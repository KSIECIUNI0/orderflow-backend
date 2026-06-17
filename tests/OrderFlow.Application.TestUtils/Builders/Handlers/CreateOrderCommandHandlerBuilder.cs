using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Abstractions;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Application.TestUtils.Builders.Handlers;

public class CreateOrderCommandHandlerBuilder : BaseServiceBuilder<CreateOrderCommandHandler>
{
    public IOrderRepository FreezeOrderRepository()
    {
        return Freeze<IOrderRepository>();
    }

    public IUnitOfWork FreezeUnitOfWork()
    {
        return Freeze<IUnitOfWork>();
    }
}