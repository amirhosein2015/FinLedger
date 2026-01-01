using FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Ledger.Tests.Integration.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Dapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Modules.Ledger.Tests.Integration.Accounts;

public class CreateAccountIntegrationTests : BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    public CreateAccountIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAccount_Should_CreateDynamicSchema_And_PersistAccount()
    {
        // Arrange
        var tenantId = "berlin_fintech";
        
        // Using a unique account code to avoid Redis lock collisions in CI environments
        var uniqueCode = $"ACT-{Guid.NewGuid().ToString()[..8]}"; 
        var command = new CreateAccountCommand(uniqueCode, "Main Cash Account", AccountType.Asset);

        // 1. Set the Tenant context
        var tenantProvider = _factory.Services.GetRequiredService<TestTenantProvider>();
        tenantProvider.TenantId = tenantId;

        // 2. Create a fresh scope
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();

        // 3. Ensure schema exists
        await dbContext.CreateSchemaAsync(tenantId);

        // Act
        var accountId = await sender.Send(command);

        // Assert
        var account = await dbContext.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == accountId);

        account.Should().NotBeNull();
        account!.Code.Should().Be(uniqueCode);

        // Physical check
        var connection = dbContext.Database.GetDbConnection();
        var schemaExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = @SchemaName)",
            new { SchemaName = tenantId.ToLower() });
        
        schemaExists.Should().BeTrue();
    }
}

