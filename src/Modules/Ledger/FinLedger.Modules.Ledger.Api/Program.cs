using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application; //ValidationBehavior
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation; 

var builder = WebApplication.CreateBuilder(args);

// ۱. اضافه کردن سرویس‌های کنترلر
builder.Services.AddControllers();

// ۲. تنظیمات Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ۳. اتصال به PostgreSQL
builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ۴. معرفی DbContext به لایه Application
builder.Services.AddScoped<ILedgerDbContext>(provider => provider.GetRequiredService<LedgerDbContext>());

// ۵. ثبت MediatR و Pipeline Behaviors (الگوی حرفه‌ای CQRS)
builder.Services.AddMediatR(cfg => 
{
    // پیدا کردن تمام Handlerها در لایه Application
    cfg.RegisterServicesFromAssembly(typeof(ILedgerDbContext).Assembly);
    
    // Principal Signal: اضافه کردن رفتار اعتبار‌سنجی خودکار قبل از اجرای هر Command
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// ۶. ثبت تمام Validatorهای FluentValidation در لایه Application
builder.Services.AddValidatorsFromAssembly(typeof(ILedgerDbContext).Assembly);

// ۷. ثبت TenantProvider و HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();

var app = builder.Build();

// ۸. فعال‌سازی Swagger در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
