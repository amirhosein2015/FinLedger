using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Asp.Versioning; // اضافه شد

var builder = WebApplication.CreateBuilder(args);

// ۱. تنظیمات Versioning (سیگنال مهندسی ارشد برای مدیریت چرخه حیات نرم‌افزار)
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

// ۲. اضافه کردن سرویس‌های کنترلر
builder.Services.AddControllers();

// ۳. تنظیمات Swagger (به‌روزرسانی شده برای پشتیبانی از نسخه‌بندی)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ۴. مدیریت خطای مرکزی (Global Exception Handling)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ۵. اتصال به PostgreSQL
builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ۶. معرفی DbContext به لایه Application
builder.Services.AddScoped<ILedgerDbContext>(provider => provider.GetRequiredService<LedgerDbContext>());

// ۷. ثبت MediatR و Pipeline Behaviors (CQRS)
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(ILedgerDbContext).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// ۸. ثبت تمام Validatorهای لایه Application
builder.Services.AddValidatorsFromAssembly(typeof(ILedgerDbContext).Assembly);

// ۹. ثبت TenantProvider و HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();

var app = builder.Build();

// ۱۰. فعال‌سازی مدیریت خطا (باید اولین Middleware باشد)
app.UseExceptionHandler();

// ۱۱. تنظیم Swagger برای نمایش نسخه‌های مختلف API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();
