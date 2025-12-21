using FinLedger.Modules.Ledger.Domain.Accounts;

namespace FinLedger.Modules.Ledger.Api.Requests;

// استفاده از Record برای خوانایی و Immutability بیشتر (سیگنال دات‌نت 9)
public record CreateAccountRequest(
    string Code, 
    string Name, 
    AccountType Type);
