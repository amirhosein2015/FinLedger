using FinLedger.BuildingBlocks.Application.Abstractions;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace FinLedger.BuildingBlocks.Infrastructure.Resilience;

public class RedisDistributedLock : IDistributedLock
{
    private readonly RedLockFactory _lockFactory;

    public RedisDistributedLock(IConnectionMultiplexer redis)
    {
        // Correct implementation for RedLock.net 2.x
        var multiplexers = new List<RedLockMultiplexer> { new RedLockMultiplexer(redis) };
        _lockFactory = RedLockFactory.Create(multiplexers);
    }

    public async Task<IDisposable?> AcquireAsync(string resourceKey, TimeSpan expiration, CancellationToken ct = default)
    {
        var redLock = await _lockFactory.CreateLockAsync(resourceKey, expiration);
        return redLock.IsAcquired ? redLock : null;
    }
}
