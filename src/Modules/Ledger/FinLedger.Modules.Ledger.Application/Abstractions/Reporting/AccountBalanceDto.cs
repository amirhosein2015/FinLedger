namespace FinLedger.Modules.Ledger.Application.Abstractions.Reporting;

/// <summary>
/// Read Model for the Trial Balance report.
/// Includes totals and final calculated balance.
/// </summary>
public record AccountBalanceDto
{
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public decimal Balance => TotalDebit - TotalCredit; // Standard accounting equation
}
