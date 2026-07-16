using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Api.Middleware;

public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            await WriteError(context, ex.Message, "Request validation failed", HttpStatusCode.BadRequest);
        }
        catch (DomainException ex)
        {
            await WriteError(context, ex.Message, ex.ErrorTitle, HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue) 
                ? headerValue.ToString() 
                : "UNKNOWN";
            
            logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);
            
            await WriteError(
                context,
                "An unexpected error occurred.", 
                "Unexpected error occurred",
                HttpStatusCode.InternalServerError);
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
        return context.Response.WriteAsJsonAsync(problem);
    }
}