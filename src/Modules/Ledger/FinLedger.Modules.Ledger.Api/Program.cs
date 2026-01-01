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

// Setting the AppContext switch BEFORE any other executable code
// to ensure PostgreSQL handles DateTime UTC conversions correctly.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Setup QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting FinLedger Enterprise API with Observability...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // --- 1. Identity Module ---
    builder.Services.AddIdentityModule(builder.Configuration);

    // --- 2. OpenTelemetry Configuration (Phase 7) ---
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => 
        {
            tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("FinLedger.API"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation() 
                .AddRedisInstrumentation() 
                .AddSource("MediatR") 
                .AddOtlpExporter(opt => 
                {
                    opt.Endpoint = new Uri("http://localhost:4317");
                });
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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

    builder.Services.AddScoped<IAuthorizationHandler, TenantRoleHandler>();
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new TenantRoleRequirement("Admin")));
        options.AddPolicy("AccountantAccess", policy => policy.Requirements.Add(new TenantRoleRequirement("Accountant")));
    });

    // --- 4. API & Controllers ---
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
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {token}",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // --- 5. MediatR & Persistence ---
    builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(ILedgerDbContext).Assembly,
            typeof(FinLedger.Modules.Identity.Application.Abstractions.IIdentityDbContext).Assembly
        );
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddValidatorsFromAssemblies(new[] { 
        typeof(ILedgerDbContext).Assembly,
        typeof(FinLedger.Modules.Identity.Application.Abstractions.IIdentityDbContext).Assembly 
    });

    // Suppressing the strict EF Core 9 migration warning for Multi-tenant environments
    builder.Services.AddDbContext<LedgerDbContext>((sp, opt) =>
    {
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        opt.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        
       
        opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });

    builder.Services.AddScoped<ILedgerDbContext>(p => p.GetRequiredService<LedgerDbContext>());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect("localhost:16379")); 
    builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

    // Registering CurrentUserProvider to track identity in audit logs
    builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .AddRedis("localhost:16379");

    var app = builder.Build();

    // --- 6. Middlewares ---
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                options.SwaggerEndpoint(url, description.GroupName.ToUpperInvariant());
            }
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
