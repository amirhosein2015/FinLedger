using Microsoft.AspNetCore.Authorization;

namespace FinLedger.BuildingBlocks.Application.Abstractions.Security;

// Custom requirement to check for specific tenant-based roles
public record TenantRoleRequirement(string RequiredRole) : IAuthorizationRequirement;
