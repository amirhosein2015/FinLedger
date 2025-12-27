using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Asp.Versioning;
using StackExchange.Redis;
using FinLedger.BuildingBlocks.Application.Abstractions;
using FinLedger.BuildingBlocks.Infrastructure.Resilience;

var builder = WebApplication.CreateBuilder(args);

// API Versioning Configuration
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

// Swagger Documentation with Tenant Header Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{
    options.OperationFilter<TenantHeaderFilter>();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinLedger Enterprise API", Version = "v1" });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Database Persistence Configuration
builder.Services.AddDbContext<LedgerDbContext>((serviceProvider, dbOptions) =>
{
    dbOptions.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    dbOptions.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
    dbOptions.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddScoped<ILedgerDbContext>(p => p.GetRequiredService<LedgerDbContext>());

// MediatR and Validation Pipeline setup
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(ILedgerDbContext).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(ILedgerDbContext).Assembly);

// Multi-tenancy Infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();



// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:16379")); 

builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

var app = builder.Build();

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

// Request Pipeline Middleware
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
