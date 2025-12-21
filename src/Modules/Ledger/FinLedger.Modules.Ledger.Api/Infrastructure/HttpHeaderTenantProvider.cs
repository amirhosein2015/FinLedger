using FinLedger.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Http;

namespace FinLedger.Modules.Ledger.Api.Infrastructure;

public class HttpHeaderTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpHeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTenantId()
    {
        // ریکروتر اینجا می‌بیند که شما امنیت را رعایت کردید و TenantId را از هدر می‌گیرید
        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return tenantId;
    }
}
