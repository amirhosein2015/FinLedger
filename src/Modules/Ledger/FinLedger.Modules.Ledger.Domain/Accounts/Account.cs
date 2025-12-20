using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Accounts;

public class Account : AggregateRoot
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public bool IsActive { get; private set; }

    private Account() { } // مخصوص EF Core

    public static Account Create(string code, string name, AccountType type)
    {
        // Principal Signal: قوانین سخت‌گیرانه در دامین
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کد حساب الزامی است.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("نام حساب الزامی است.");

        return new Account
        {
            Code = code,
            Name = name,
            Type = type,
            IsActive = true
        };
    }
}
