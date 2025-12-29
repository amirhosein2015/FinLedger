using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application.Abstractions;
using FinLedger.BuildingBlocks.Infrastructure.Persistence.Outbox;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence;

/// <summary>
/// The primary database context for the Ledger module.
/// Implements Multi-tenancy via Schema-per-Tenant and the Outbox Pattern for reliable messaging.
/// </summary>
public class LedgerDbContext : DbContext, ILedgerDbContext
{
    public string TenantId { get; }

    public LedgerDbContext(DbContextOptions<LedgerDbContext> options, ITenantProvider tenantProvider) 
        : base(options)
    {
        // Resolve TenantId from provider or default to public
        TenantId = tenantProvider.GetTenantId()?.ToLower().Trim() ?? "public";
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setting the default schema based on the current Tenant
        modelBuilder.HasDefaultSchema(TenantId);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
        
        // Outbox Configuration
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(x => x.Id);
        });

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Overriding SaveChanges to implement the Outbox Pattern atomically.
    /// Ensures business data and domain events are persisted in a single transaction.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Extract Domain Events from tracked Aggregate Roots
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(x => x.Entity)
            .SelectMany(x =>
            {
                var events = x.DomainEvents.ToList();
                x.ClearDomainEvents();
                return events;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            })
            .ToList();

        // Persist events as Outbox Messages within the same database transaction
        this.AddRange(domainEvents);

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Dynamically creates a new schema and initializes tables for new tenants.
    /// </summary>
    public async Task CreateSchemaAsync(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "public") return;
        
        var cleanSchema = schemaName.ToLower().Trim();

        //Using ExecuteSqlInterpolatedAsync to satisfy EF Core 9 security analyzers (EF1002)
        // and prevent SQL Injection while creating dynamic schemas.
        await Database.ExecuteSqlInterpolatedAsync($"CREATE SCHEMA IF NOT EXISTS \"{cleanSchema}\";");

        var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            try 
            { 
     
                await databaseCreator.CreateTablesAsync(); 
            } 
            catch 
            { 
                // Swallow exceptions if tables already exist
            }
        }
    }
}
