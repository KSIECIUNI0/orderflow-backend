using MediatR;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ProductName, request.Quantity);
        var saved = await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new OrderResponse(saved.Id, saved.ProductName, saved.Quantity);
    }
}