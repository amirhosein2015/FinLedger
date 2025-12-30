using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Tests.Integration.Abstractions;

public class TestTenantProvider : ITenantProvider
{
    public string? TenantId { get; set; } = "public";

    public string? GetTenantId() => TenantId;
}
