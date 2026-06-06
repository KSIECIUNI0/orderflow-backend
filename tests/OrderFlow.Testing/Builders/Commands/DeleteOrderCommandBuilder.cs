using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Testing.Builders.Commands;

public class DeleteOrderCommandBuilder
{
    private Guid _orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public DeleteOrderCommandBuilder WithOrderId(Guid value)
    {
        _orderId = value;
        return this;
    }

    public DeleteOrderCommand Build()
    {
        return new DeleteOrderCommand(_orderId);
    }
}