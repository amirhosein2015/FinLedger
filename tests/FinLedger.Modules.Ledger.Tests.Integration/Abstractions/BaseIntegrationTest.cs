using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using Xunit;

namespace FinLedger.Modules.Ledger.Tests.Integration.Abstractions;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory Factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
    }

    // Method to create a clean scope for each test scenario
    protected IServiceScope CreateScope() => Factory.Services.CreateScope();
}
