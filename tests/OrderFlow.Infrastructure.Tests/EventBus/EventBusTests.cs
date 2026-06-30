using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.TestUtils.Builders.EventBus;

namespace OrderFlow.Infrastructure.Tests.EventBus;

public class EventBusTests
{
    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldThrowAnArgumentNullException_WhenOrderCreatedEventIsNull()
    {
        // Arrange
        var correlationId = $"{Guid.NewGuid()}";
        var sut = new EventBusBuilder().Build();
        
        // Act
        var act = () => sut.PublishOrderCreatedAsync(null!, correlationId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("event");
    }

    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldPrepareOutgoingMessageAndPublishEvent_WhenCallWithValidParams()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var correlationId = $"{Guid.NewGuid()}";
        var @event = new OrderCreatedEvent(orderId);
        var expectedOutgoingMessageBody = JsonSerializer.Serialize(@event);
        
        var builder = new EventBusBuilder();
        var messagePublisher = builder.FreezeMessagePublisher();
        var sut = builder.Build();
        
        // Act
        await sut.PublishOrderCreatedAsync(@event, correlationId, TestContext.Current.CancellationToken);
        
        // Assert
        await messagePublisher
            .Received(1)
            .PublishAsync(
                Arg.Is<OutgoingMessage>(m =>
                    m.QueueIdentifier == QueueIdentifiers.OrderCreated &&
                    m.Body == expectedOutgoingMessageBody &&
                    m.CorrelationId == correlationId),
                Arg.Any<CancellationToken>());
    }
}