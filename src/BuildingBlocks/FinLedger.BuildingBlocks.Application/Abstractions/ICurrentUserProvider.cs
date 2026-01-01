namespace FinLedger.BuildingBlocks.Application.Abstractions;

public interface ICurrentUserProvider
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}
