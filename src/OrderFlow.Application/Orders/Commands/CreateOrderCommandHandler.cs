using MediatR;
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

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ProductName, request.Quantity);
        var saved = await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var @event = new OrderCreatedEvent(saved.Id, saved.ProductName, saved.Quantity);
        await _eventBus.PublishAsync(@event, cancellationToken);
        
        return new OrderResponse(saved.Id, saved.ProductName, saved.Quantity);
    }
}