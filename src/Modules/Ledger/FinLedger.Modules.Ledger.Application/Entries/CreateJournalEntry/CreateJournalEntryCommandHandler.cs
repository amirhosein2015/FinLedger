using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Entries;
using MediatR;

namespace FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;

// Sealed class prevents inheritance and allows the compiler to perform direct dispatch
internal sealed class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;

    public CreateJournalEntryCommandHandler(ILedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Mapping DTOs to Domain-friendly tuples before passing to the Aggregate Root
        var lines = request.Lines
            .Select(l => (l.AccountId, l.Debit, l.Credit))
            .ToList();

        // Create the aggregate root which enforces strict double-entry accounting invariants
        var entry = JournalEntry.Create(request.TransactionDate, request.Description, lines);
        
        _dbContext.JournalEntries.Add(entry);
        
        // The Outbox pattern is handled inside SaveChangesAsync to ensure atomicity
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
