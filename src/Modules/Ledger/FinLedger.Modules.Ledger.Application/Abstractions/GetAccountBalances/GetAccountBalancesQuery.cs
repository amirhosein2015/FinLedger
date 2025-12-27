using Dapper;
using FinLedger.Modules.Ledger.Application.Abstractions;
using FinLedger.Modules.Ledger.Application.Abstractions.Reporting;
using MediatR;
using Microsoft.EntityFrameworkCore; // Very important for GetDbConnection
using System.Data;

namespace FinLedger.Modules.Ledger.Application.Accounts.GetAccountBalances;

public record GetAccountBalancesQuery : IRequest<IReadOnlyCollection<AccountBalanceDto>>;

internal class GetAccountBalancesQueryHandler : IRequestHandler<GetAccountBalancesQuery, IReadOnlyCollection<AccountBalanceDto>>
{
    private readonly ILedgerDbContext _dbContext;

    public GetAccountBalancesQueryHandler(ILedgerDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyCollection<AccountBalanceDto>> Handle(GetAccountBalancesQuery request, CancellationToken cancellationToken)
    {
        // Principal Signal: Accessing the underlying relational connection for Dapper
        if (_dbContext is not DbContext efContext)
        {
            throw new InvalidOperationException("Database context is not a relational context.");
        }

        var connection = efContext.Database.GetDbConnection();
        
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync(cancellationToken);

        var schema = _dbContext.TenantId;

        // Optimized SQL for high-performance reporting
        var sql = $@"
            SELECT 
                a.""Code"" as AccountCode, 
                a.""Name"" as AccountName,
                COALESCE(SUM(l.""Debit""), 0) as TotalDebit,
                COALESCE(SUM(l.""Credit""), 0) as TotalCredit
            FROM ""{schema}"".""Accounts"" a
            LEFT JOIN ""{schema}"".""JournalEntryLines"" l ON a.""Id"" = l.""AccountId""
            LEFT JOIN ""{schema}"".""JournalEntries"" e ON l.""JournalEntryId"" = e.""Id""
            WHERE e.""Status"" = 2 -- 2 = Posted
               OR e.""Status"" IS NULL -- Include accounts even without entries
            GROUP BY a.""Code"", a.""Name""
            ORDER BY a.""Code"";";

        var result = await connection.QueryAsync<AccountBalanceDto>(sql);
        return result.ToList().AsReadOnly();
    }
}
