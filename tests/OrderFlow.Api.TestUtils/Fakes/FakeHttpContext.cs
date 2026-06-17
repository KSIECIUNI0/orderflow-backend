using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace OrderFlow.Api.TestUtils.Fakes;

public class FakeHttpContext : HttpContext
{
    private FakeHttpResponse _fakeHttpResponse = default!;
    
    public override void Abort()
    {
        throw new NotImplementedException();
    }
    
    public void SetFakeHttpResponce(FakeHttpResponse fakeHttpResponse) => _fakeHttpResponse = fakeHttpResponse;

    public override HttpResponse Response => _fakeHttpResponse;
    public override IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();
    
    public override IFeatureCollection Features => default!;
    public override HttpRequest Request => default!;
    public override ConnectionInfo Connection => default!;
    public override WebSocketManager WebSockets => default!;
    public override ClaimsPrincipal User { get; set; } = default!;
    public override IServiceProvider RequestServices { get; set; } = default!;
    public override CancellationToken RequestAborted { get; set; } = default!;
    public override string TraceIdentifier { get; set; } = default!;
    public override ISession Session { get; set; } = default!;
}