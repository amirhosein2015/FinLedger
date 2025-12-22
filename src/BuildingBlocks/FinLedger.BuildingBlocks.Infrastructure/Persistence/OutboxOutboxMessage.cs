using System;

namespace FinLedger.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Represents a message to be processed and sent to the message broker.
/// Part of the Outbox Pattern to ensure transactional integrity.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
