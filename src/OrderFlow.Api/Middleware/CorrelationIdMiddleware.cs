using OrderFlow.Application.Common.Interfaces;
using Serilog.Context;

namespace OrderFlow.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor accessor)
    {
        const string headerName = "X-Correlation-Id";
        
        var correlationId = context.Request.Headers.TryGetValue(headerName, out var headerValue) 
            ? headerValue.ToString() 
            : Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            accessor.Set(correlationId);
            await _next(context);
        }
    }
}