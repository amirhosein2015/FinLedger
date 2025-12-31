using FinLedger.Modules.Identity.Application.Abstractions; // Added this missing using
using FinLedger.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Persistence implementation for the Identity module.
/// Implements IIdentityDbContext to remain decoupled from the application layer.
/// </summary>
public sealed class IdentityDbContext : DbContext, IIdentityDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity schema is shared across tenants
        modelBuilder.HasDefaultSchema("identity");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}

