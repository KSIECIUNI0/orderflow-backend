using MediatR;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Worker.Orders.Handlers;

public class OrderCreatedSqsHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedSqsHandler> _logger;

    public OrderCreatedSqsHandler(ILogger<OrderCreatedSqsHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing OrderCreatedEvent for OrderId={OrderId}", notification.OrderId);

        return Task.CompletedTask;
    }
}