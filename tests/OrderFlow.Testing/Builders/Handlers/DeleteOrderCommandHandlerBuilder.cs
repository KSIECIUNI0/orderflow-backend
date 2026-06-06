using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Testing.Builders.Handlers;

public class DeleteOrderCommandHandlerBuilder : BaseServiceBuilder<DeleteOrderCommandHandler>
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