using FinLedger.Modules.Identity.Application.Abstractions;
using FinLedger.Modules.Identity.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Identity.Application.Users.AssignTenantRole;

public record AssignTenantRoleCommand(Guid UserId, string TenantId, UserRole Role) : IRequest;

internal sealed class AssignTenantRoleCommandHandler(IIdentityDbContext dbContext) 
    : IRequestHandler<AssignTenantRoleCommand>
{
    public async Task Handle(AssignTenantRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.TenantRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) throw new KeyNotFoundException("User not found.");

        // Executing domain logic to maintain invariants
        user.AssignToTenant(request.TenantId, request.Role);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
