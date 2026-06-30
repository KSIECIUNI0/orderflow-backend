using FluentAssertions;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Infrastructure.TestUtils.Builders.Messaging;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;

namespace OrderFlow.Infrastructure.Tests.Messaging;

public class QueueIdentifierResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ResolveQueueUrl_ShouldThrowAnArgumentException_WhenQueueIdentifierIsNullOrEmpty(
        string? queueIdentifier)
    {
        // Arrange
        var sut = new QueueIdentifierResolverBuilder().Build();
        
        // Act
        var act = () => sut.ResolveQueueUrl(queueIdentifier);
        
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("queueIdentifier");
    }
    
    [Fact]
    public void ResolveQueueUrl_ShouldThrowAnArgumentOutOfRangeException_WhenQueueIdentifierIsInvalid()
    {
        // Arrange
        const string queueIdentifier = "invalid_queue_identifier"; 
        var sut = new QueueIdentifierResolverBuilder().Build();
        
        // Act
        var act = () => sut.ResolveQueueUrl(queueIdentifier);
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("queueIdentifier");
    }

    [Theory]
    [InlineData("order_created_queue_url", "order_created_dlq_url", QueueIdentifiers.OrderCreated, "order_created_queue_url")]
    [InlineData("order_created_queue_url", "order_created_dlq_url", QueueIdentifiers.OrderCreatedDlq, "order_created_dlq_url")]
    public void ResolveQueueUrl_ShouldReturnUrl_WhenCallWithValidQueueIdentifier(
        string orderCreatedQueueUrl,
        string orderCreatedDlqUrl,
        string queueIdentifier,
        string expectedUrl)
    {
        // Arrange
        var orderCreatedQueueOptions = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(orderCreatedQueueUrl)
            .Build();
        var orderCreatedDlqOptions = new OrderCreatedDlqOptionsBuilder()
            .WithUrl(orderCreatedDlqUrl)
            .Build();
        var sut = new QueueIdentifierResolverBuilder()
            .WithOrderCreatedQueueOptions(orderCreatedQueueOptions)
            .WithOrderCreatedDlqOptions(orderCreatedDlqOptions)
            .Build();
        
        // Act
        var result = sut.ResolveQueueUrl(queueIdentifier);
        
        // Assert
        result.Should().Be(expectedUrl);
    }
}