using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Entries;
using MediatR;

namespace FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;

internal class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;

    public CreateJournalEntryCommandHandler(ILedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Principal Signal: Transform DTO to Domain logic
        var lines = request.Lines
            .Select(l => (l.AccountId, l.Debit, l.Credit))
            .ToList();

        // Create the aggregate root which enforces accounting invariants
        var entry = JournalEntry.Create(request.TransactionDate, request.Description, lines);

        _dbContext.JournalEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
