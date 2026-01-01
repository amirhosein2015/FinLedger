using MediatR;
using FinLedger.Modules.Ledger.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Ledger.Application.Accounts.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;
public record AuditLogDto(Guid Id, Guid UserId, string Action, string EntityName, string Changes, DateTime OccurredOnUtc);

internal sealed class GetAuditLogsQueryHandler(ILedgerDbContext dbContext) 
    : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(x => x.OccurredOnUtc)
            .Select(x => new AuditLogDto(x.Id, x.UserId, x.Action, x.EntityName, x.Changes!, x.OccurredOnUtc))
            .ToListAsync(cancellationToken);
    }
}
