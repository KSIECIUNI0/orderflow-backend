using Microsoft.AspNetCore.Http;

namespace OrderFlow.Api.TestUtils.Fakes;

public class FakeHttpResponse : HttpResponse
{
    public override void OnStarting(Func<object, Task> callback, object state)
    {
        throw new NotImplementedException();
    }

    public override void OnCompleted(Func<object, Task> callback, object state)
    {
        throw new NotImplementedException();
    }

    public override void Redirect(string location, bool permanent)
    {
        throw new NotImplementedException();
    }

    public override IHeaderDictionary Headers { get; } = new HeaderDictionary();

    public override HttpContext HttpContext => default!;
    public override int StatusCode { get; set; } = default!;
    public override Stream Body { get; set; } = default!;
    public override long? ContentLength { get; set; } = default!;
    public override string? ContentType { get; set; } = default!;
    public override IResponseCookies Cookies => default!;
    public override bool HasStarted => default!;
}