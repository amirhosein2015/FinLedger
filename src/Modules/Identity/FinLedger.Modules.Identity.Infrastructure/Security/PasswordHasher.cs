using FinLedger.Modules.Identity.Application.Abstractions;

namespace FinLedger.Modules.Identity.Infrastructure.Security;

internal sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) => 
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
