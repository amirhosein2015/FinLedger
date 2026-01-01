using FinLedger.BuildingBlocks.Domain;
using FinLedger.BuildingBlocks.Application.Abstractions;
using FinLedger.BuildingBlocks.Infrastructure.Persistence.Outbox;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using FinLedger.Modules.Ledger.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence;

public class LedgerDbContext : DbContext, ILedgerDbContext
{
    private readonly ICurrentUserProvider _currentUserProvider;
    public string TenantId { get; }

    public LedgerDbContext(
        DbContextOptions<LedgerDbContext> options, 
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider) 
        : base(options)
    {
        TenantId = tenantProvider.GetTenantId()?.ToLower().Trim() ?? "public";
        _currentUserProvider = currentUserProvider;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(TenantId);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
        
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(x => x.Id);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges();

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

        this.AddRange(domainEvents);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog or OutboxMessage) continue;

            var auditLog = AuditLog.Create(
                _currentUserProvider.UserId,
                entry.State.ToString(),
                entry.Entity.GetType().Name,
                (entry.Entity as Entity)?.Id ?? Guid.Empty,
                JsonSerializer.Serialize(entry.CurrentValues.ToObject())
            );

            AuditLogs.Add(auditLog);
        }
    }

    /// <summary>
    /// High-reliability schema provisioning using RelationalDatabaseCreator.
    /// This ensures tables are physically created in the tenant schema even if migrations are out of sync.
    /// </summary>
    public async Task CreateSchemaAsync(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "public") return;
        
        var cleanSchema = schemaName.ToLower().Trim();

        // 1. Physically create the schema
        await Database.ExecuteSqlInterpolatedAsync($"CREATE SCHEMA IF NOT EXISTS \"{cleanSchema}\";");

        // 2. Get the low-level database creator service
        var databaseCreator = (RelationalDatabaseCreator)Database.GetService<IDatabaseCreator>();
        
        try 
        {
            // 3. Force create tables based on the current model in this specific schema
            await databaseCreator.CreateTablesAsync();
        } 
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07")
        {
            // 42P07 = relation already exists. This is fine, it means tables are already there.
        }
        catch (Exception)
        {
            // Handle or log other potential creation issues
        }
    }
}

