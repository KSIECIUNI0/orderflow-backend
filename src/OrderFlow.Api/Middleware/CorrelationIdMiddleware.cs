using OrderFlow.Application.Correlation;
using Serilog.Context;

namespace OrderFlow.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor accessor)
    {
        const string headerName = "X-Correlation-Id";
        
        var correlationId = context.Request.Headers.TryGetValue(headerName, out var headerValue) 
            ? headerValue.ToString() 
            : Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            accessor.Set(correlationId);
            await next(context);
        }
    }
}