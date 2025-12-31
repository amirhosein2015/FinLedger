using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Identity.Infrastructure; 
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
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using QuestPDF.Infrastructure;

// Setup QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting FinLedger Enterprise API with Identity Support...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // --- 1. Identity & Security Configuration ---
    builder.Services.AddIdentityModule(builder.Configuration);

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

    builder.Services.AddAuthorization();

    // --- 2. API & Infrastructure Configuration ---
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

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen(options => 
    {
        options.OperationFilter<TenantHeaderFilter>();
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinLedger Enterprise API", Version = "v1" });

        // Adding JWT Authorization support to Swagger UI
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // --- 3. Database & Modules Persistence ---
    builder.Services.AddDbContext<LedgerDbContext>((serviceProvider, dbOptions) =>
    {
        dbOptions.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        dbOptions.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        dbOptions.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
    builder.Services.AddScoped<ILedgerDbContext>(p => p.GetRequiredService<LedgerDbContext>());

    // MediatR & Shared Behaviors
    builder.Services.AddMediatR(cfg => 
    {
        // Register from both modules
        cfg.RegisterServicesFromAssemblies(
            typeof(ILedgerDbContext).Assembly,
            typeof(IdentityModule).Assembly);
        
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddValidatorsFromAssemblies(new[] { 
        typeof(ILedgerDbContext).Assembly,
        typeof(IdentityModule).Assembly 
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect("localhost:16379")); 
    builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "PostgreSQL")
        .AddRedis("localhost:16379", name: "Redis Cache");

    var app = builder.Build();

    // --- 4. Middleware Pipeline ---
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

    // Authentication must come before Authorization
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
