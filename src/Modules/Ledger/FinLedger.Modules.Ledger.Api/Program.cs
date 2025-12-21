using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ۱. اضافه کردن سرویس‌های کنترلر
builder.Services.AddControllers();

// ۲. تنظیمات Swagger برای تست API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ۳. اتصال به PostgreSQL
builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ۴. ثبت TenantProvider (که قبلاً در گام اول ساختیم)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();

var app = builder.Build();

// ۵. فعال‌سازی Swagger در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
