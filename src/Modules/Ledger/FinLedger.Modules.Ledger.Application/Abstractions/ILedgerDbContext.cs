using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Abstractions;

public interface ILedgerDbContext
{
    string TenantId { get; }
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
