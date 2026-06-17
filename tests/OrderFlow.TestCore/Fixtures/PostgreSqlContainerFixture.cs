using Testcontainers.PostgreSql;
using Xunit;

namespace OrderFlow.TestCore.Fixtures;

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

    public async ValueTask InitializeAsync()
        => await _container.StartAsync();

    public async ValueTask DisposeAsync()
        => await _container.StopAsync();
}