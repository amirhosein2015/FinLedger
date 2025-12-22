using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;

namespace FinLedger.Modules.Ledger.Api.Infrastructure;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, LedgerDbContext dbContext)
    {
        var tenantId = tenantProvider.GetTenantId();

        if (!string.IsNullOrEmpty(tenantId) && tenantId != "public")
        {
   
            await dbContext.CreateSchemaAsync(tenantId);
        }

        await _next(context);
    }
}
