using MediatR;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Entries.ReverseJournalEntry;

public record ReverseJournalEntryCommand(Guid Id, string Reason) : IRequest<Guid>;

// Implementing immutability via Reversal logic instead of physical deletion
internal sealed class ReverseJournalEntryCommandHandler : IRequestHandler<ReverseJournalEntryCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;
    public ReverseJournalEntryCommandHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task<Guid> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Loading the original entry to perform a safe reversal
        var entry = await _dbContext.JournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entry == null) throw new KeyNotFoundException("Original entry not found.");

        // Using the Zero-Delete Policy. Corrections are made via automated reversal.
        // This ensures a 100% reliable audit trail for compliance.
        var reversalEntry = entry.CreateReversal(request.Reason);
        
        _dbContext.JournalEntries.Add(reversalEntry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return reversalEntry.Id;
    }
}
