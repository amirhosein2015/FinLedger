using FinLedger.BuildingBlocks.Application.Abstractions; 
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using MediatR;

namespace FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;

//Using 'sealed' to improve performance via devirtualization
internal sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;
    private readonly IDistributedLock _distributedLock; 

    public CreateAccountCommandHandler(ILedgerDbContext dbContext, IDistributedLock distributedLock)
    {
        _dbContext = dbContext;
        _distributedLock = distributedLock; 
    }

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Define a unique lock key per Tenant and Account Code
        var lockKey = $"lock:tenant:{_dbContext.TenantId}:account:{request.Code}";
        
        // Attempt to acquire the distributed lock for 10 seconds to ensure consistency
        using (var handle = await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(10)))
        {
            if (handle == null)
            {
                // Concurrency Guard: Prevent duplicate account codes in highly distributed environments
                throw new InvalidOperationException($"The account code '{request.Code}' is currently being processed.");
            }

            var account = Account.Create(request.Code, request.Name, request.Type);
            
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return account.Id;
        }
    }
}
