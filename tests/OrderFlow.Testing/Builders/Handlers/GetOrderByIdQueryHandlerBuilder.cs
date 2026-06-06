using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Testing.Builders.Handlers;

public class GetOrderByIdQueryHandlerBuilder : BaseServiceBuilder<GetOrderByIdQueryHandler>
{
    public IOrderRepository FreezeOrderRepository()
    {
        return Freeze<IOrderRepository>();
    }
}