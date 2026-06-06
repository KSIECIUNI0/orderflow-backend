using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Testing.Builders.Handlers;

public class GetAllOrdersQueryHandlerBuilder : BaseServiceBuilder<GetAllOrdersQueryHandler>
{
    public IOrderRepository FreezeOrderRepository()
    {
        return Freeze<IOrderRepository>();
    }
}