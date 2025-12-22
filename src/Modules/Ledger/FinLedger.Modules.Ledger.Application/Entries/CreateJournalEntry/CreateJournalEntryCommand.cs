using MediatR;

namespace FinLedger.Modules.Ledger.Application.Entries.CreateJournalEntry;

public record CreateJournalEntryCommand(
    string Description,
    DateTime TransactionDate,
    List<JournalEntryLineDto> Lines) : IRequest<Guid>;

public record JournalEntryLineDto(
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string? Memo);
