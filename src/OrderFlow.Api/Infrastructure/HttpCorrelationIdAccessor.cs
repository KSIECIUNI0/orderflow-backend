using OrderFlow.Application.Correlation;

namespace OrderFlow.Api.Infrastructure;

public class HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdAccessor
{
    private const string HeaderName = "X-Correlation-Id";

    public string? CorrelationId
    {
        get
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx is null) 
                return null;
            
            return ctx.Items.TryGetValue(HeaderName, out var headerValue) 
                ? headerValue?.ToString() 
                : null;
        }
    }
    public void Set(string correlationId)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null) 
            return;
        
        ctx.Items[HeaderName] = correlationId;
        ctx.Response.Headers[HeaderName] = correlationId;
    }
}