using FinLedger.Modules.Identity.Application.Abstractions;
using FinLedger.Modules.Identity.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Modules.Identity.Application.Users.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Guid>;

internal sealed class RegisterUserCommandHandler(
    IIdentityDbContext dbContext,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new InvalidOperationException("Email is already in use.");

        var user = User.Create(request.Email, passwordHasher.Hash(request.Password), request.FirstName, request.LastName);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
