using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure;
using Testcontainers.PostgreSql;

namespace OrderFlow.IntegrationTests.Fixtures;

public class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgreSqlContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("orderflow")
            .WithUsername("orderflow")
            .WithPassword("orderflow")
            .Build();
    }

    public string ConnectionString 
        => _container.GetConnectionString();

    public async Task InitializeAsync()
        => await _container.StartAsync();

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
        await _container.StopAsync();
    }

    private async Task ClearDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<OrderFlowDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var db = new OrderFlowDbContext(options);

        db.Orders.RemoveRange(db.Orders);
        await db.SaveChangesAsync();
    }
}