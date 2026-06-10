using WireMock.Server;
using Xunit;

namespace OrderFlow.Testing.Fixtures;

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
}