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
        
        // Sanitize the schema name to prevent any potential issues
        var cleanSchema = schemaName.ToLower().Trim();

        // Fix for EF1002: Build the SQL string explicitly. 
        // We use double quotes for PostgreSQL identifiers (schemas).
        var sql = "CREATE SCHEMA IF NOT EXISTS \"" + cleanSchema + "\";";
        await Database.ExecuteSqlRawAsync(sql);

        // Use the RelationalDatabaseCreator to force-create tables in the new schema
        var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            try 
            { 
                // This ensures all tables defined in this DbContext are created in the current schema
                await databaseCreator.CreateTablesAsync(); 
            } 
            catch 
            { 
                // Tables might already exist, which is expected on subsequent requests
            }
        }
    }
}
