using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Accounts.GetAccountBalances;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Generates a summary of all accounts with their total debits, credits, and final balances.
    /// Only includes finalized (Posted) transactions.
    /// </summary>
    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances()
    {
        var result = await _mediator.Send(new GetAccountBalancesQuery());
        return Ok(result);
    }
}
