using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ProductName, request.Quantity);
        var saved = await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order created in DB with ID {OrderId}", saved.Id);
        
        var @event = new OrderCreatedEvent(OrderId: saved.Id);
        await eventBus.PublishOrderCreatedAsync(@event, request.CorrelationId, cancellationToken);
        logger.LogInformation("OrderCreatedEvent sent to queue for Order:{OrderId} with CorrelationId:{CorrelationId}", saved.Id, request.CorrelationId);
        
        return new OrderResponse(saved.Id, saved.ProductName, saved.Quantity);
    }
}