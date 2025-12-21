using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Api.Infrastructure;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Application.Abstractions; // اضافه شد
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ۱. اضافه کردن سرویس‌های کنترلر
builder.Services.AddControllers();

// ۲. تنظیمات Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ۳. اتصال به PostgreSQL
builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ۴. معرفی DbContext به لایه Application (بسیار مهم برای CQRS)
builder.Services.AddScoped<ILedgerDbContext>(provider => provider.GetRequiredService<LedgerDbContext>());

// ۵. ثبت MediatR (به سیستم می‌گوید تمام Handlerها را در پروژه Application پیدا کند)
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(ILedgerDbContext).Assembly);
});

// ۶. ثبت TenantProvider
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();

var app = builder.Build();

// ۷. فعال‌سازی Swagger در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
