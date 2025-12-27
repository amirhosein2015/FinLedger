using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Domain.Entries;
using MediatR;

namespace FinLedger.Modules.Ledger.Application.Accounts.SeedDemoData;

public record SeedDemoDataCommand : IRequest<string>;

internal class SeedDemoDataCommandHandler : IRequestHandler<SeedDemoDataCommand, string>
{
    private readonly ILedgerDbContext _dbContext;
    public SeedDemoDataCommandHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task<string> Handle(SeedDemoDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Create two accounts with unique codes based on timestamp to avoid duplicates
        var suffix = DateTime.Now.Ticks.ToString().Substring(10);
        var cash = Account.Create($"101-{suffix}", "Demo Cash", AccountType.Asset);
        var bank = Account.Create($"102-{suffix}", "Demo Bank", AccountType.Asset);

        _dbContext.Accounts.AddRange(cash, bank);

        // 2. Create and Post a balanced Journal Entry
        var lines = new List<(Guid AccountId, decimal Debit, decimal Credit)>
        {
            (cash.Id, 0, 15000), // Credit Cash
            (bank.Id, 15000, 0)  // Debit Bank
        };

        var entry = JournalEntry.Create(DateTime.UtcNow, "Initial Seed Transaction", lines);
        entry.Post();

        _dbContext.JournalEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return "Success! Demo data created and posted. You can now download the PDF.";
    }
}
