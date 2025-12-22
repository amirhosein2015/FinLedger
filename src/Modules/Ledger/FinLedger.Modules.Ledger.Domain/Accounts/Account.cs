using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Domain.Accounts.Events;

namespace FinLedger.Modules.Ledger.Domain.Accounts;

public class Account : AggregateRoot
{
    public string Code { get; private set; } = default!; // اصلاح شد
    public string Name { get; private set; } = default!; // اصلاح شد
    public AccountType Type { get; private set; }
    public bool IsActive { get; private set; }

    private Account() { } // مخصوص EF Core

   public static Account Create(string code, string name, AccountType type)
{
    // ... validations ...

    var account = new Account
    {
        Code = code,
        Name = name,
        Type = type,
        IsActive = true
    };

    // Register the domain event
    account.AddDomainEvent(new AccountCreatedDomainEvent(account.Id, account.Code));

    return account;
}

}
