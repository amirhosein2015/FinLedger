using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using FinLedger.Modules.Ledger.Domain.Auditing; // Added for Phase 8
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Abstractions;

public interface ILedgerDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<AuditLog> AuditLogs { get; } //  Exposing Audit Logs to the Application layer

    string TenantId { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CreateSchemaAsync(string schemaName);
}
