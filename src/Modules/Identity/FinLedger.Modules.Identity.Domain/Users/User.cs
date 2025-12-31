using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Identity.Domain.Users;

public sealed class User : AggregateRoot
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    
    // Multi-tenant user mapping
    // A user can have different roles in different tenants
    private readonly List<UserTenantRole> _tenantRoles = new();
    public IReadOnlyCollection<UserTenantRole> TenantRoles => _tenantRoles.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        // ... validations ...
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }

    public void AssignToTenant(string tenantId, UserRole role)
    {
        if (_tenantRoles.Any(x => x.TenantId == tenantId))
            throw new InvalidOperationException("User is already assigned to this tenant.");

        _tenantRoles.Add(new UserTenantRole(Id, tenantId, role));
    }
}

public sealed class UserTenantRole
{
    public Guid UserId { get; private set; }
    public string TenantId { get; private set; }
    public UserRole Role { get; private set; }

    internal UserTenantRole(Guid userId, string tenantId, UserRole role)
    {
        UserId = userId;
        TenantId = tenantId;
        Role = role;
    }
}
