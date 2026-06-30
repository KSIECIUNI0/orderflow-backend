using System.Text.Json;
using OrderFlow.Application.EventBus;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Infrastructure.EventBus;

public class EventBus(IMessagePublisher messagePublisher) : IEventBus
{
    public async Task PublishOrderCreatedAsync(
        OrderCreatedEvent @event,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));

        var message = new OutgoingMessage
        {
            QueueIdentifier = QueueIdentifiers.OrderCreated,
            Body = JsonSerializer.Serialize(@event),
            CorrelationId = correlationId,
        };
        
        await messagePublisher.PublishAsync(message, cancellationToken);
    }
}