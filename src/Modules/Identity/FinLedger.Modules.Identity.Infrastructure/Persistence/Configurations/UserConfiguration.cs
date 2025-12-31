using FinLedger.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique(); // Global uniqueness for identity

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        // Configuring the collection of Tenant Roles as a separate table
        builder.OwnsMany(x => x.TenantRoles, tr =>
        {
            tr.ToTable("UserTenantRoles");
            tr.HasKey("UserId", "TenantId"); // Composite Key
            tr.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
            tr.Property(x => x.Role).HasConversion<int>();
        });
    }
}
