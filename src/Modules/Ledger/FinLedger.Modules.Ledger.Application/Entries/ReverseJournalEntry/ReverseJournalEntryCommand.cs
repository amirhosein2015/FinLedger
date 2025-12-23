using MediatR;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Entries.ReverseJournalEntry;

public record ReverseJournalEntryCommand(Guid Id, string Reason) : IRequest<Guid>;

internal class ReverseJournalEntryCommandHandler : IRequestHandler<ReverseJournalEntryCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;

    public ReverseJournalEntryCommandHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task<Guid> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.JournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entry == null) throw new KeyNotFoundException("Original entry not found.");

       
        var reversalEntry = entry.CreateReversal(request.Reason);

        _dbContext.JournalEntries.Add(reversalEntry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return reversalEntry.Id;
    }


}
