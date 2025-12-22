namespace FinLedger.BuildingBlocks.Application.Abstractions;

public interface IDistributedLock
{
    // A safe way to acquire a lock across multiple instances
    Task<IDisposable?> AcquireAsync(string resourceKey, TimeSpan expiration, CancellationToken ct = default);
}
