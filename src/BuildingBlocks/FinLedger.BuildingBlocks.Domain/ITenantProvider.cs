namespace FinLedger.BuildingBlocks.Domain;

public interface ITenantProvider
{
    string? GetTenantId(); // مثلاً "tenant_a" یا "tenant_b"
}
