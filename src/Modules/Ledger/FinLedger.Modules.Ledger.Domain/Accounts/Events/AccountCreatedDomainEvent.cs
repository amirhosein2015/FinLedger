using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Accounts.Events;

public record AccountCreatedDomainEvent(Guid AccountId, string Code) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}
