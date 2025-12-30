using FinLedger.BuildingBlocks.Domain; // Added this using
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace FinLedger.Modules.Ledger.Tests.Integration.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("finledger_test")
        .WithUsername("admin")
        .WithPassword("SecurePassword123!")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Swap TenantProvider with our Test version
            var tenantDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvider));
            if (tenantDescriptor != null) services.Remove(tenantDescriptor);

            services.AddSingleton<TestTenantProvider>();
            services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<TestTenantProvider>());

            // Swap DbContext to use the TestContainer PostgreSQL
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LedgerDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            services.AddDbContext<LedgerDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
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
