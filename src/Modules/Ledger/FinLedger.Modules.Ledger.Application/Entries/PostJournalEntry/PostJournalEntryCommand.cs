using MediatR;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Entries.PostJournalEntry;

public record PostJournalEntryCommand(Guid Id) : IRequest;

// Using 'sealed' to improve performance and prevent unintended inheritance
internal sealed class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand>
{
    private readonly ILedgerDbContext _dbContext;
    public PostJournalEntryCommandHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Load the aggregate root with its lines to perform state transitions
        var entry = await _dbContext.JournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entry == null) throw new KeyNotFoundException("Entry not found.");

        //  Domain Logic is encapsulated within the Aggregate Root.
        // The handler only orchestrates the process and triggers the domain method.
        entry.Post();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
