using Testcontainers.PostgreSql;

namespace OrderFlow.Api.IntegrationTests.Fixtures;

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
        => await _container.StopAsync();
}