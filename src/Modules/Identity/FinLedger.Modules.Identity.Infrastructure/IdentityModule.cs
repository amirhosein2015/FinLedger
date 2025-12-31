using FinLedger.Modules.Identity.Application.Abstractions;
using FinLedger.Modules.Identity.Infrastructure.Persistence;
using FinLedger.Modules.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Persistence
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. Security Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        // 3. MediatR (Registering handlers from this assembly)
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(IdentityModule).Assembly));

        return services;
    }
}
