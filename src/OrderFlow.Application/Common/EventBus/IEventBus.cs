using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Application.Common.EventBus;

public interface IEventBus
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default);
}