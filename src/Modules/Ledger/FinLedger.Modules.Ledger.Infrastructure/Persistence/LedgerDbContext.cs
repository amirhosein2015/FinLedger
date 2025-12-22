using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence;

public class LedgerDbContext : DbContext, ILedgerDbContext
{
    public string TenantId { get; }

    public LedgerDbContext(DbContextOptions<LedgerDbContext> options, ITenantProvider tenantProvider) 
        : base(options)
    {
        TenantId = tenantProvider.GetTenantId()?.ToLower().Trim() ?? "public";
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(TenantId);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task CreateSchemaAsync(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "public") return;

        var cleanSchema = schemaName.ToLower().Trim();
        
        // Ensure the schema exists in PostgreSQL
        await Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{cleanSchema}\";");
        
        // Use the RelationalDatabaseCreator to force-create tables in the new schema
        var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            try 
            { 
                await databaseCreator.CreateTablesAsync(); 
            } 
            catch 
            { 
                // Tables might already exist, which is fine
            }
        }
    }
}
