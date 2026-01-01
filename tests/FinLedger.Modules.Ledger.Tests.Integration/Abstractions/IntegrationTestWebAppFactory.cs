using FinLedger.BuildingBlocks.Domain;
using FinLedger.Modules.Ledger.Infrastructure.Persistence;
using FinLedger.Modules.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Dynamically overriding connection strings for the entire Host.
            // We add 'abortConnect=false' to Redis to ensure the host doesn't crash during OTEL initialization.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString() + ",abortConnect=false",
                ["Jwt:Secret"] = "CI_Test_Secret_Key_At_Least_32_Chars_Long!!",
                ["Jwt:Issuer"] = "FinLedger",
                ["Jwt:Audience"] = "FinLedgerUsers"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // 1. Remove and Re-register IdentityDbContext to use the TestContainer
            var identityDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (identityDescriptor != null) services.Remove(identityDescriptor);
            services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

            // 2. Remove and Re-register LedgerDbContext to use the TestContainer
            var ledgerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LedgerDbContext>));
            if (ledgerDescriptor != null) services.Remove(ledgerDescriptor);
            services.AddDbContext<LedgerDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

            // 3. Fix the Scoped/Singleton conflict by providing a controllable Tenant Provider
            var tenantDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvider));
            if (tenantDescriptor != null) services.Remove(tenantDescriptor);

            services.AddSingleton<TestTenantProvider>();
            services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<TestTenantProvider>());
        });
    }

    public async Task InitializeAsync()
    {
        // Start containers in parallel to speed up CI execution
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_dbContainer.StopAsync(), _redisContainer.StopAsync());
    }
}
