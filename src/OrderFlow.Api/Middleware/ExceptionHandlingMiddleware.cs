using System.Net;
using System.Text.Json;
using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Api.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            await WriteError(context, ex.Message, HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            await WriteError(context, "Unexpected server error occurred", HttpStatusCode.InternalServerError);
        }
    }

    private static Task WriteError(HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}