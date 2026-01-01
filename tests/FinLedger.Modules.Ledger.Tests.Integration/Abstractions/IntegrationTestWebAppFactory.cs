using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Identity.Infrastructure.Persistence; // Added for Identity
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration; // Added for Config override
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace FinLedger.Modules.Ledger.Tests.Integration.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //  Overriding the configuration globally
        // This ensures BOTH Ledger and Identity modules use the same TestContainer instance
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Jwt:Secret"] = "Test_Secret_Key_For_CI_Pipeline_123456789",
                ["Jwt:Issuer"] = "FinLedger",
                ["Jwt:Audience"] = "FinLedgerUsers"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // 1. Ensure our TestTenantProvider is registered
            var tenantDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvider));
            if (tenantDescriptor != null) services.Remove(tenantDescriptor);

            services.AddSingleton<TestTenantProvider>();
            services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<TestTenantProvider>());

            // 2. Re-register DbContexts to use the new connection string from container
            // This is a safety measure to ensure EF Core picks up the right driver
            services.AddDbContext<LedgerDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));
            services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
