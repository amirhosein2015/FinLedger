namespace FinLedger.Modules.Ledger.Application.Abstractions.Reporting;

/// <summary>
/// Read Model for the Trial Balance report.
/// Includes totals and final calculated balance.
/// </summary>

public class AccountBalanceDto
{
    public Guid Id { get; set; } 
    public string AccountCode { get; set; } = default!;
    public string AccountName { get; set; } = default!;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance => TotalDebit - TotalCredit;
}
