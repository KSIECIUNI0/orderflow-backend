using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        RequestDelegate next = _ => throw new FakeDomainException(exceptionMessage);
        
        var sut = new ExceptionHandlingMiddleware();
        
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
        
        var sut = new ExceptionHandlingMiddleware();
        
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
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        const string exceptionMessage = "Exception";
        RequestDelegate next = _ => throw new Exception(exceptionMessage);
        
        var sut = new ExceptionHandlingMiddleware();
        
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
        problem.Title.Should().Be("Unexpected error occured");
        problem.Detail.Should().Be(exceptionMessage);
    }
}