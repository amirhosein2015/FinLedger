using Asp.Versioning;
using FinLedger.Modules.Ledger.Api.Infrastructure.Reports;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Application.Accounts.GetAccountBalances;
using FinLedger.Modules.Ledger.Application.Accounts.SeedDemoData;
using FinLedger.Modules.Ledger.Application.Accounts.GetAuditLogs; 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Ledger.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ledger/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILedgerDbContext _dbContext;

    public ReportsController(IMediator mediator, ILedgerDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the account balances in JSON format.
    /// </summary>
    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances()
    {
        var result = await _mediator.Send(new GetAccountBalancesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Generates and downloads a professional PDF Trial Balance report.
    /// </summary>
    [HttpGet("balances/pdf")]
    public async Task<IActionResult> GetBalancesPdf()
    {
        var balances = await _mediator.Send(new GetAccountBalancesQuery());
        
        // Use the TenantId from DbContext which is already resolved by Middleware
        var currentTenant = _dbContext.TenantId;
        
        var pdfBytes = TrialBalancePdfGenerator.Generate(currentTenant, balances);
        
        return File(pdfBytes, "application/pdf", $"TrialBalance_{currentTenant}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Seeds demo data for the current tenant to quickly test reporting.
    /// </summary>
    [HttpPost("seed-demo-data")]
    public async Task<IActionResult> Seed()
    {
        var result = await _mediator.Send(new SeedDemoDataCommand());
        return Ok(new { Message = result });
    }

    /// <summary>
    /// Returns the system audit logs for compliance tracking.
    /// Access: Administrators and Auditors.
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        // Principal Signal: Leveraging the centralized Audit Log table for financial transparency
        var result = await _mediator.Send(new GetAuditLogsQuery());
        return Ok(result);
    }
}

