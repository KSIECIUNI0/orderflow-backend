using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Messaging;

public interface IEventBus
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent @event, string? correlationId, CancellationToken cancellationToken = default);
}