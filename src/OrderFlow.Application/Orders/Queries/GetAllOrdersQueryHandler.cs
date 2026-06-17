using MediatR;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders
            .Select(order => new OrderResponse(order.Id, order.ProductName, order.Quantity))
            .ToList();
    }
}