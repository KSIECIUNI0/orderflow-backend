using OrderFlow.Application.Common.Interfaces;

namespace OrderFlow.Api.Infrastructure;

public class HttpCorrelationIdAccessor : ICorrelationIdAccessor
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) 
                return null;
            
            return ctx.Items.TryGetValue(HeaderName, out var headerValue) 
                ? headerValue?.ToString() 
                : null;
        }
    }
    public void Set(string correlationId)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null) 
            return;
        
        ctx.Items[HeaderName] = correlationId;
        ctx.Response.Headers[HeaderName] = correlationId;
    }
}