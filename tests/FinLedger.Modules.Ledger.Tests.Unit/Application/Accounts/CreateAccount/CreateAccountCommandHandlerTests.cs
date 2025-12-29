using FinLedger.BuildingBlocks.Application.Abstractions;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinLedger.Modules.Ledger.Tests.Unit.Application.Accounts.CreateAccount;

public class CreateAccountCommandHandlerTests
{
    private readonly ILedgerDbContext _dbContext;
    private readonly IDistributedLock _distributedLock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        // Principal Signal: Using NSubstitute for clean and decoupled mocking
        _dbContext = Substitute.For<ILedgerDbContext>();
        _distributedLock = Substitute.For<IDistributedLock>();
        
        _handler = new CreateAccountCommandHandler(_dbContext, _distributedLock);
    }

    [Fact]
    public async Task Handle_Should_AcquireLock_And_CreateAccount_When_RequestIsValid()
    {
        // Arrange
        var command = new CreateAccountCommand("101", "Cash", AccountType.Asset);
        
        // Mocking the lock acquisition to return a disposable handle (success)
        _distributedLock.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(Substitute.For<IDisposable>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        // Verify that the distributed lock was requested for this specific account code
        await _distributedLock.Received(1).AcquireAsync(
            Arg.Is<string>(s => s.Contains("101")), 
            Arg.Any<TimeSpan>());

        // Verify that the account was added to the DbContext
        _dbContext.Accounts.Received(1).Add(Arg.Is<Account>(a => a.Code == "101"));

        // Verify that changes were persisted
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_LockCannotBeAcquired()
    {
        // Arrange
        var command = new CreateAccountCommand("102", "Bank", AccountType.Asset);
        
        // Mocking lock failure (returns null)
        _distributedLock.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns((IDisposable?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*currently being processed*");

        // Ensure no data was saved if the lock failed
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
