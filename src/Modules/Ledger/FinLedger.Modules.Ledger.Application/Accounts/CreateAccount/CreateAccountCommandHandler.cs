using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Domain.Accounts;
using MediatR;

namespace FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;

// این کلاس مسئول اجرای دستور ساخت حساب است
internal class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly ILedgerDbContext _dbContext;

    public CreateAccountCommandHandler(ILedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // ۱. ساخت موجودیت با استفاده از قوانین Domain (رعایت DDD)
        var account = Account.Create(request.Code, request.Name, request.Type);

        // ۲. اضافه کردن به دیتابیس (از طریق اینترفیس)
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // ۳. برگرداندن شناسه
        return account.Id;
    }
}
