using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Identity.Infrastructure;
using FinLedger.Modules.Ledger.Api.Infrastructure.Security;
using FinLedger.BuildingBlocks.Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Asp.Versioning;
using StackExchange.Redis;
using FinLedger.BuildingBlocks.Application.Abstractions;
using FinLedger.BuildingBlocks.Infrastructure.Resilience;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using QuestPDF.Infrastructure;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// 1. Ensure PostgreSQL UTC compatibility
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting FinLedger Enterprise API with React Integration...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddIdentityModule(builder.Configuration);

    // --- 2. OpenTelemetry & Tracing ---
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => 
        {
            tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("FinLedger.API"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation() 
                .AddRedisInstrumentation() 
                .AddSource("MediatR") 
                .AddOtlpExporter(opt => { opt.Endpoint = new Uri("http://localhost:4317"); });
        });

    // --- 3. Security & Authorization ---
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

    builder.Services.AddScoped<IAuthorizationHandler, TenantRoleHandler>();
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new TenantRoleRequirement("Admin")));
        options.AddPolicy("AccountantAccess", policy => policy.Requirements.Add(new TenantRoleRequirement("Accountant")));
    });

    // Configuring CORS to allow the React Frontend to communicate with the API
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins("http://localhost:5174") // Vite Dev Server URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // --- 4. API Configuration ---
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddControllers()
        .AddApplicationPart(typeof(FinLedger.Modules.Identity.Api.Controllers.UsersController).Assembly);

    builder.Services.AddSwaggerGen(options => 
    {
        options.OperationFilter<TenantHeaderFilter>();
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinLedger API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Type = SecuritySchemeType.Http, BearerFormat = "JWT", Scheme = "bearer" });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // --- 5. MediatR & Persistence ---
    builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssemblies(typeof(ILedgerDbContext).Assembly, typeof(FinLedger.Modules.Identity.Application.Abstractions.IIdentityDbContext).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddValidatorsFromAssemblies(new[] { typeof(ILedgerDbContext).Assembly, typeof(FinLedger.Modules.Identity.Application.Abstractions.IIdentityDbContext).Assembly });

    builder.Services.AddDbContext<LedgerDbContext>((sp, opt) =>
    {
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        opt.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });


    
    builder.Services.AddScoped<ILedgerDbContext>(p => p.GetRequiredService<LedgerDbContext>());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
    builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

    // --- 6. Resilience: Dynamic Redis Configuration ---
    var redisUrl = builder.Configuration.GetConnectionString("Redis") ?? "localhost:16379";
    var redisOptions = ConfigurationOptions.Parse(redisUrl);
    redisOptions.AbortOnConnectFail = false; 
    redisOptions.ConnectRetry = 5;
    redisOptions.ConnectTimeout = 10000;

    var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .AddRedis(redisUrl);

    var app = builder.Build();

    // --- 7. Pipeline ---
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    // Enable CORS before Authentication/Authorization
    app.UseCors("AllowReactApp");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();
            foreach (var description in descriptions)
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        });
    }

    app.MapHealthChecks("/health");
    app.UseMiddleware<TenantMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
