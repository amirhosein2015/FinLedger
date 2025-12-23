using MediatR;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Entries.PostJournalEntry;

public record PostJournalEntryCommand(Guid Id) : IRequest;

internal class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand>
{
    private readonly ILedgerDbContext _dbContext;

    public PostJournalEntryCommandHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.JournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entry == null) throw new KeyNotFoundException("Entry not found.");

        entry.Post();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
