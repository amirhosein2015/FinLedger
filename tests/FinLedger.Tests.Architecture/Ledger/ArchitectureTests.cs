using NetArchTest.Rules;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace FinLedger.Tests.Architecture.Ledger;

public class ArchitectureTests
{
    private const string DomainNamespace = "FinLedger.Modules.Ledger.Domain";
    private const string ApplicationNamespace = "FinLedger.Modules.Ledger.Application";
    private const string InfrastructureNamespace = "FinLedger.Modules.Ledger.Infrastructure";

    private static readonly Assembly DomainAssembly = typeof(FinLedger.Modules.Ledger.Domain.Accounts.Account).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(FinLedger.Modules.Ledger.Application.Abstractions.ILedgerDbContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(FinLedger.Modules.Ledger.Infrastructure.Persistence.LedgerDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Layers()
    {
        // Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

       
        result.IsSuccessful.Should().BeTrue("The Domain layer must be the pure core of the system.");
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure()
    {
        // Act
        // Using HaveDependencyOnAny even for a single namespace
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("The Application layer should only depend on the Domain.");
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        // Ensuring Command/Query handlers are sealed for performance
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("All Command/Query handlers must be sealed for performance optimization.");
    }
}
