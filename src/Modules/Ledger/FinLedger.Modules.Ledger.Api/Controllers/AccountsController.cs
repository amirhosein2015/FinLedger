using FinLedger.Modules.Ledger.Api.Requests;
using FinLedger.Modules.Ledger.Domain.Accounts;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiController]
[Route("api/ledger/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly LedgerDbContext _dbContext;

    public AccountsController(LedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        // ۱. ساخت موجودیت بر اساس قوانین دامین
        var account = Account.Create(request.Code, request.Name, request.Type);

        // ۲. اضافه کردن به دیتابیس
        // نکته حرفه‌ای: اینجا EF Core به صورت خودکار از Schema-per-Tenant استفاده می‌کند
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return Ok(new { account.Id, account.Code, account.Name });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _dbContext.Accounts.ToListAsync();
        return Ok(accounts);
    }
}
