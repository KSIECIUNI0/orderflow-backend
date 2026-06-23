using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
        catch (Exception ex) when (
            ex is DomainException or ArgumentException)
        {
            await WriteError(context, ex.Message, "Request validation failed", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            await WriteError(context, ex.Message, "Unexpected error occured", HttpStatusCode.BadRequest);
        }
    }

    private static Task WriteError(
        HttpContext context,
        string message,
        string title,
        HttpStatusCode statusCode)
    {
        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = message
        };
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}