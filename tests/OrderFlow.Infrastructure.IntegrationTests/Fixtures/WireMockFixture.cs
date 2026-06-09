using WireMock.Server;

namespace OrderFlow.Infrastructure.IntegrationTests.Fixtures;

public class WireMockFixture : IAsyncLifetime
{
    public WireMockServer Server { get; private set; } = null!;
    
    public ValueTask InitializeAsync()
    {
        Server = WireMockServer.Start();
        return ValueTask.CompletedTask;
    }
    
    public ValueTask DisposeAsync()
    {
        Server.Stop();
        Server.Dispose();
        
        return ValueTask.CompletedTask;
    }
    
    public HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(Server.Url!)
        };
    }
}