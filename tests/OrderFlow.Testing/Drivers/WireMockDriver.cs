using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OrderFlow.Testing.Drivers;

public class WireMockDriver(WireMockServer server)
{
    public HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(server.Url!)
        };
    }

    public ValueTask ResetLogEntries()
    {
        server.ResetLogEntries();
        return ValueTask.CompletedTask;
    }
    
    public void ConfigureSuccessfulResponse(string path)
    {
        ConfigureResponse(path, 200);
    }
    
    public void ConfigureInternalServerError(string path)
    {
        ConfigureResponse(path, 500);
    }

    private void ConfigureResponse(string path, int statusCode)
    {
        server
            .Given(
                Request.Create()
                    .WithPath(path)
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(statusCode));
    }
}