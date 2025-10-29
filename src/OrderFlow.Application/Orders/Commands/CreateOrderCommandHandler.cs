using MediatR;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ProductName, request.Quantity);
        var saved = await _orderRepository.AddAsync(order, cancellationToken);
        return new OrderResponse(saved.Id, saved.ProductName, saved.Quantity);
    }
}