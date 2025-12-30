using FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Infrastructure.Persistence; // For LedgerDbContext
using FinLedger.Modules.Ledger.Tests.Integration.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Dapper;
using MediatR; // For ISender
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Modules.Ledger.Tests.Integration.Accounts;

public class CreateAccountIntegrationTests : BaseIntegrationTest
{
    public CreateAccountIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAccount_Should_CreateDynamicSchema_And_PersistAccount()
    {
        // Arrange
        var tenantId = "berlin_fintech";
        var command = new CreateAccountCommand("101", "Main Cash Account", AccountType.Asset);

        // Access the TestTenantProvider to set the context
        var tenantProvider = Factory.Services.GetRequiredService<TestTenantProvider>();
        tenantProvider.TenantId = tenantId;

        // Create a fresh scope for this specific test execution
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();

        // Ensure the physical schema exists in the PostgreSQL container
        await dbContext.CreateSchemaAsync(tenantId);

        // Act
        // Send the command through the MediatR pipeline
        var accountId = await sender.Send(command);

        // Assert
        // Verify account exists in the database within the correct schema
        var account = await dbContext.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == accountId);

        account.Should().NotBeNull();
        account!.Code.Should().Be("101");

        // Physical Integrity Check: Verify schema existence via Dapper
        var connection = dbContext.Database.GetDbConnection();
        var schemaExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = @SchemaName)",
            new { SchemaName = tenantId.ToLower() });
        
        schemaExists.Should().BeTrue("a physical database schema should be created for the new tenant.");
    }
}
