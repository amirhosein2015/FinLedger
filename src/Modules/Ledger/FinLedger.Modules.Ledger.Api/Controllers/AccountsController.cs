using FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FinLedger.Modules.Ledger.Api.Controllers;


[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
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
       
        var accountId = await _mediator.Send(command);
        
        return Ok(new { Id = accountId });
    }
}
