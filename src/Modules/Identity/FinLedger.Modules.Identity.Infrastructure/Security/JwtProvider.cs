using System.Security.Claims;
using System.Text;
using FinLedger.Modules.Identity.Application.Abstractions;
using FinLedger.Modules.Identity.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace FinLedger.Modules.Identity.Infrastructure.Security;

internal sealed class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    public string Create(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", $"{user.FirstName} {user.LastName}")
        };

        // Packing Multi-tenant roles into JWT claims
        // Format: "tenant_id:role" (e.g., "berlin_fintech:Admin")
        foreach (var tenantRole in user.TenantRoles)
        {
            claims.Add(new Claim("tenant_access", $"{tenantRole.TenantId}:{tenantRole.Role}"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
