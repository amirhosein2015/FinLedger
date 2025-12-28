using FinLedger.Modules.Ledger.Application.Abstractions.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinLedger.Modules.Ledger.Api.Infrastructure.Reports;

public static class TrialBalancePdfGenerator
{
    public static byte[] Generate(string tenantName, IReadOnlyCollection<AccountBalanceDto> balances)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                // Document Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Trial Balance Report").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Tenant: {tenantName}").FontSize(12).Italic().FontColor(Colors.Grey.Darken2);
                    });

                    row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontSize(10);
                });

                // Content Area
                page.Content().PaddingVertical(10).Column(column => 
                {
                    if (balances == null || !balances.Any())
                    {
                        column.Item().PaddingTop(20).AlignCenter().Text("No posted transactions found for this tenant.")
                            .FontSize(14).FontColor(Colors.Red.Medium);
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            // Correct Column Definitions
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Account
                                columns.RelativeColumn(1); // Debit
                                columns.RelativeColumn(1); // Credit
                                columns.RelativeColumn(1); // Balance
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Account");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total Debit");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total Credit");
                                header.Cell().Element(CellStyle).AlignRight().Text("Balance");

                                static IContainer CellStyle(IContainer container) => 
                                    container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            // Table Data Rows
                            foreach (var item in balances)
                            {
                                table.Cell().Element(RowStyle).Text($"{item.AccountCode} - {item.AccountName}");
                                table.Cell().Element(RowStyle).AlignRight().Text(item.TotalDebit.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(item.TotalCredit.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(item.Balance.ToString("N2"));

                                static IContainer RowStyle(IContainer container) => 
                                    container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            }
                        });
                    }
                });

                // Page Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
