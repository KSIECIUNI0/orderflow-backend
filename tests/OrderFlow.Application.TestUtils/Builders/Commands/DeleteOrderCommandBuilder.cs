using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Application.TestUtils.Builders.Commands;

public class DeleteOrderCommandBuilder
{
    private Guid _orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public DeleteOrderCommandBuilder WithOrderId(Guid value)
    {
        _orderId = value;
        return this;
    }

    public DeleteOrderCommand Build() => new(_orderId);
}