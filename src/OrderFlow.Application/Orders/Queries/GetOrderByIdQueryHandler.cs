using MediatR;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);

        return order is null 
            ? null 
            : new OrderResponse(order.Id, order.ProductName, order.Quantity);
    }
}