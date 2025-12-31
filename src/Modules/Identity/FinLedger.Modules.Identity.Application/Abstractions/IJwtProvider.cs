using FinLedger.Modules.Identity.Domain.Users;

namespace FinLedger.Modules.Identity.Application.Abstractions;

public interface IJwtProvider
{
    string Create(User user);
}
