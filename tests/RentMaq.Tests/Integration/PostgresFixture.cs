using Microsoft.EntityFrameworkCore;
using RentMaq.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RentMaq.Tests.Integration;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4")
        .WithDatabase("rentmaq_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public RentMaqDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<RentMaqDbContext>()
            .UseNpgsql(_container.GetConnectionString(),
                npgsql => npgsql.UseNetTopologySuite())
            .Options;

        DbContext = new RentMaqDbContext(options);
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
