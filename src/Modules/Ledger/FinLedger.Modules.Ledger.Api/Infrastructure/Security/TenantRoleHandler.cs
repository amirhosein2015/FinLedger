using FinLedger.BuildingBlocks.Application.Abstractions.Security;
using FinLedger.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Authorization;

namespace FinLedger.Modules.Ledger.Api.Infrastructure.Security;

internal sealed class TenantRoleHandler(ITenantProvider tenantProvider) 
    : AuthorizationHandler<TenantRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        TenantRoleRequirement requirement)
    {
        // 1. Resolve current tenant from header
        var currentTenant = tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(currentTenant)) return Task.CompletedTask;

        // 2.  An 'Admin' should have access to everything within their tenant.
        // We check for both the specific required role OR the 'Admin' role.
        var requiredClaim = $"{currentTenant}:{requirement.RequiredRole}";
        var adminClaim = $"{currentTenant}:Admin";

        var hasAccess = context.User.Claims.Any(c => 
            c.Type == "tenant_access" && 
            (c.Value.Equals(requiredClaim, StringComparison.OrdinalIgnoreCase) || 
             c.Value.Equals(adminClaim, StringComparison.OrdinalIgnoreCase)));

        if (hasAccess)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
