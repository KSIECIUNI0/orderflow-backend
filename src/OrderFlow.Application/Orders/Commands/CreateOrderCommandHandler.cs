using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.EventBus;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ProductName, request.Quantity);
        var saved = await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order created in DB with ID {OrderId}", saved.Id);
        
        var @event = new OrderCreatedEvent(OrderId: saved.Id);
        await _eventBus.PublishOrderCreatedAsync(@event, request.CorrelationId, cancellationToken);
        _logger.LogInformation("OrderCreatedEvent sent to SQS for Order:{OrderId} with CorrelationId:{CorrelationId}", saved.Id, request.CorrelationId);
        
        return new OrderResponse(saved.Id, saved.ProductName, saved.Quantity);
    }
}