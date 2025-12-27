using Asp.Versioning;
using FinLedger.Modules.Ledger.Application.Accounts.GetAccountBalances;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FinLedger.Modules.Ledger.Api.Infrastructure.Reports;
using FinLedger.Modules.Ledger.Application.Accounts.SeedDemoData;

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


[HttpGet("balances/pdf")]
public async Task<IActionResult> GetBalancesPdf()
{
    var balances = await _mediator.Send(new GetAccountBalancesQuery());
    
    // Principal Signal: Using the real tenant ID from the context
    var tenantId = (string)HttpContext.Request.Headers["X-Tenant-Id"]!;
    
    var pdfBytes = TrialBalancePdfGenerator.Generate(tenantId, balances);
    
    return File(pdfBytes, "application/pdf", $"TrialBalance_{tenantId}_{DateTime.Now:yyyyMMdd}.pdf");
}




[HttpPost("seed-demo-data")]
public async Task<IActionResult> Seed()
{
    var result = await _mediator.Send(new SeedDemoDataCommand());
    return Ok(new { Message = result });
}





}
