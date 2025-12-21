using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence;

public class LedgerDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public LedgerDbContext(DbContextOptions<LedgerDbContext> options, ITenantProvider tenantProvider) 
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) // اصلاح نام ModelBuilder
    {
        // استراتژی جداسازی مشتریان
        var tenantId = _tenantProvider.GetTenantId() ?? "public";
        modelBuilder.HasDefaultSchema(tenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
public void EnsureSchemaCreated()
{
    var tenantId = _tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId) || tenantId == "public") return;

    // ایجاد اسکما در صورتی که وجود نداشته باشد
    Database.ExecuteSqlRaw($"CREATE SCHEMA IF NOT EXISTS {tenantId};");
   
}



}
