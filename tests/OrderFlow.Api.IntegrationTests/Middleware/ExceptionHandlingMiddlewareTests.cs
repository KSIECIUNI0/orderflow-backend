using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OrderFlow.Api.Middleware;
using OrderFlow.Api.TestUtils.Fakes;

namespace OrderFlow.Api.IntegrationTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldWriteError_WhenRequestDelegateThrowADomainException()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        const string exceptionMessage = "Domain exception";
        var fakeDomainException = new FakeDomainException(exceptionMessage);
        RequestDelegate next = _ => throw fakeDomainException;

        var logger = new FakeLogger<ExceptionHandlingMiddleware>();
        var sut = new ExceptionHandlingMiddleware(logger);
        
        // Act
        await sut.InvokeAsync(context, next);
        
        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().StartWith("application/json");
        
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
        
        problem.Should().NotBeNull();
        problem.Status.Should().Be(400);
        problem.Title.Should().Be(fakeDomainException.ErrorTitle);
        problem.Detail.Should().Be(exceptionMessage);
    }
    
    [Fact]
    public async Task InvokeAsync_ShouldWriteError_WhenRequestDelegateThrowAnArgumentException()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        const string exceptionMessage = "Argument exception";
        RequestDelegate next = _ => throw new ArgumentException(exceptionMessage);
        
        var logger = new FakeLogger<ExceptionHandlingMiddleware>();
        var sut = new ExceptionHandlingMiddleware(logger);
        
        // Act
        await sut.InvokeAsync(context, next);
        
        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().StartWith("application/json");
        
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
        
        problem.Should().NotBeNull();
        problem.Status.Should().Be(400);
        problem.Title.Should().Be("Request validation failed");
        problem.Detail.Should().Be(exceptionMessage);
    }
    
    [Fact]
    public async Task InvokeAsync_ShouldWriteError_WhenRequestDelegateThrowAnException()
    {
        // Arrange
        const string requestMethod = "POST";
        const string requestPath = "/orders";
        var correlationId = $"{Guid.NewGuid()}";
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
        context.Request.Headers.Append("X-Correlation-Id", correlationId);
        context.Request.Method = requestMethod;
        context.Request.Path = requestPath;

        const string exceptionMessage = "Exception";
        RequestDelegate next = _ => throw new Exception(exceptionMessage);
        
        var logger = new FakeLogger<ExceptionHandlingMiddleware>();
        var sut = new ExceptionHandlingMiddleware(logger);
        
        // Act
        await sut.InvokeAsync(context, next);
        
        // Assert
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().StartWith("application/json");
        
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
        
        problem.Should().NotBeNull();
        problem.Status.Should().Be(500);
        problem.Title.Should().Be("Unexpected error occurred");
        problem.Detail.Should().Be("An unexpected error occurred.");

        var log = logger.Collector.GetSnapshot()
            .FirstOrDefault(l => l.Level == LogLevel.Error);

        log.Should().NotBeNull();
        log.State.Should().NotBeNull();
        var logState = log.State
            .Should()
            .BeAssignableTo<IEnumerable<KeyValuePair<string, string>>>()
            .Subject;
        logState
            .Should()
            .ContainKeys("Method", "Path", "CorrelationId")
            .And
            .ContainValues(requestMethod, requestPath, correlationId);
    }
}