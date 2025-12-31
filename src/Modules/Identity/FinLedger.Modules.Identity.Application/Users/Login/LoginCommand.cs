using FinLedger.Modules.Identity.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Identity.Application.Users.Login;

public record LoginCommand(string Email, string Password) : IRequest<string>;

internal sealed class LoginCommandHandler(
    IIdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, string>
{
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.TenantRoles)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return jwtProvider.Create(user);
    }
}
