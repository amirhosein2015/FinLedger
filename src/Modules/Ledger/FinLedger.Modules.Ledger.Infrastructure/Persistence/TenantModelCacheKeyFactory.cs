using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence;

public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        // Using a safe way to get the TenantId from the context itself
        // to avoid Scoped/Singleton dependency injection conflicts.
        var tenantId = (context as LedgerDbContext)?.TenantId ?? "public";

        return (context.GetType(), tenantId, designTime);
    }
}
