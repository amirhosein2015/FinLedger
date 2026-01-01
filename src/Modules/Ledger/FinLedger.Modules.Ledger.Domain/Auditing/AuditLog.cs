using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Auditing;

// Immutable Audit Log for Compliance
public sealed class AuditLog : Entity
{
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string EntityName { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string? Changes { get; private set; } // JSON of changed values
    public DateTime OccurredOnUtc { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid userId, string action, string entityName, Guid entityId, string? changes)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes,
            OccurredOnUtc = DateTime.UtcNow
        };
    }
}
