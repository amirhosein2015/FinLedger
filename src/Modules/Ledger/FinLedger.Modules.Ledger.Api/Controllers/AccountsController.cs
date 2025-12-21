using FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiController]
[Route("api/ledger/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountCommand command)
    {
        //  کنترلر فقط پیام را به MediatR می‌دهد
        var accountId = await _mediator.Send(command);
        
        return Ok(new { Id = accountId });
    }
}
