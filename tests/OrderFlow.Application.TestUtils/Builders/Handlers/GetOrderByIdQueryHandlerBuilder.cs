using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Application.TestUtils.Builders.Handlers;

public class GetOrderByIdQueryHandlerBuilder : BaseServiceBuilder<GetOrderByIdQueryHandler>
{
    public IOrderRepository FreezeOrderRepository()
    {
        return Freeze<IOrderRepository>();
    }
}