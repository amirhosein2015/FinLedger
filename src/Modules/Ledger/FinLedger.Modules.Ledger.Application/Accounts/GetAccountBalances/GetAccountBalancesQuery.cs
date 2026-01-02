using Dapper;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Application.Abstractions.Reporting;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FinLedger.Modules.Ledger.Application.Accounts.GetAccountBalances;

public record GetAccountBalancesQuery : IRequest<IReadOnlyCollection<AccountBalanceDto>>;

internal sealed class GetAccountBalancesQueryHandler(ILedgerDbContext dbContext) 
    : IRequestHandler<GetAccountBalancesQuery, IReadOnlyCollection<AccountBalanceDto>>
{
    public async Task<IReadOnlyCollection<AccountBalanceDto>> Handle(GetAccountBalancesQuery request, CancellationToken cancellationToken)
    {
        if (dbContext is not DbContext efContext)
            throw new InvalidOperationException("Relational database context is required.");

        var connection = efContext.Database.GetDbConnection();
        if (connection.State == ConnectionState.Closed) await connection.OpenAsync(cancellationToken);

        var schema = dbContext.TenantId;

        // Added a."Id" to the SELECT statement to ensure the Frontend can identify accounts
        var sql = $@"
            SELECT 
                a.""Id"" as Id, 
                a.""Code"" as AccountCode, 
                a.""Name"" as AccountName,
                COALESCE(SUM(CASE WHEN e.""Status"" = 2 THEN l.""Debit"" ELSE 0 END), 0) as TotalDebit,
                COALESCE(SUM(CASE WHEN e.""Status"" = 2 THEN l.""Credit"" ELSE 0 END), 0) as TotalCredit
            FROM ""{schema}"".""Accounts"" a
            LEFT JOIN ""{schema}"".""JournalEntryLines"" l ON a.""Id"" = l.""AccountId""
            LEFT JOIN ""{schema}"".""JournalEntries"" e ON l.""JournalEntryId"" = e.""Id""
            GROUP BY a.""Id"", a.""Code"", a.""Name""
            ORDER BY a.""Code"";";

        var result = await connection.QueryAsync<AccountBalanceDto>(sql);

        return result.ToList().AsReadOnly();
    }
}
