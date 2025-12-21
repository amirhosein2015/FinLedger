using MediatR;
using FinLedger.Modules.Ledger.Domain.Accounts;

namespace FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;


public record CreateAccountCommand(
    string Code, 
    string Name, 
    AccountType Type) : IRequest<Guid>; 
