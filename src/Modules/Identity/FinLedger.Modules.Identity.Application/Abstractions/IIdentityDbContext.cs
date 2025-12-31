using FinLedger.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Identity.Application.Abstractions;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
