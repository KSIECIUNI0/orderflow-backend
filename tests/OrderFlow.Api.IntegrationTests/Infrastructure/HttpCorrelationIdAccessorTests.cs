using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OrderFlow.Api.TestUtils.Builders.Infrastructure;
using OrderFlow.Api.TestUtils.Fakes;

namespace OrderFlow.Api.IntegrationTests.Infrastructure;

public class HttpCorrelationIdAccessorTests
{
    [Fact]
    public void CorrelationId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();

        httpContextAccessor
            .HttpContext
            .Returns(null as HttpContext);
        
        var sut = builder.Build();
        
        // Act
        var result = sut.CorrelationId;
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_ShouldReturnNull_WhenHttpContextDoesNotContainCorrelationIdHeader()
    {
        // Arrange
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();
        var context = new DefaultHttpContext();

        httpContextAccessor
            .HttpContext
            .Returns(context);
        
        var sut = builder.Build();
        
        // Act
        var result = sut.CorrelationId;
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_ShouldReturnNull_WhenHttpContextDoesNotHaveValueForCorrelationIdHeader()
    {
        // Arrange
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();

        var context = new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>
            {
                { "X-Correlation-Id", null }
            }
        };

        httpContextAccessor
            .HttpContext
            .Returns(context);
        
        var sut = builder.Build();
        
        // Act
        var result = sut.CorrelationId;
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_ShouldReturnCorrelationId()
    {
        // Arrange
        const string correlationId = "abcd1234";
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();

        var context = new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>
            {
                { "X-Correlation-Id",  correlationId }
            }
        };

        httpContextAccessor
            .HttpContext
            .Returns(context);
        
        var sut = builder.Build();
        
        // Act
        var result = sut.CorrelationId;
        
        // Assert
        result.Should().Be(correlationId);
    }

    [Fact]
    public void Set_ShouldSetCorrelationId()
    {
        // Arrange
        const string correlationId = "abcd1234";
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();
        var context = new DefaultHttpContext();

        httpContextAccessor
            .HttpContext
            .Returns(context);
        
        var sut = builder.Build();
        
        // Act
        sut.Set(correlationId);
        
        // Assert
        context.Items.Should()
            .ContainKey("X-Correlation-Id")
            .And
            .ContainValue(correlationId);

        var responseHeader = context.Response.Headers["X-Correlation-Id"];
        responseHeader.Should().NotBeNullOrEmpty();
        responseHeader.Should().Contain(correlationId);
    }
    
    [Fact]
    public void Set_ShouldNotThrow_WhenHttpContextIsNull()
    {
        // Arrange
        var builder = new HttpCorrelationIdAccessorBuilder();
        var httpContextAccessor = builder.FreezeHttpContextAccessor();

        httpContextAccessor
            .HttpContext
            .Returns(null as HttpContext);
        
        var sut = builder.Build();

        // Act
        var act = () => sut.Set("abc123");

        // Assert
        act.Should().NotThrow();
    }
}