using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Application.TestUtils.Builders.Commands;

public class UpdateOrderCommandBuilder
{
    private const string ValidProductName = "New Product Name";
    
    private Guid _id = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private string _productName = ValidProductName;
    private int _quantity = 5;

    public UpdateOrderCommandBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }
    
    public UpdateOrderCommandBuilder WithProductName(string value)
    {
        _productName = value;
        return this;
    }

    public UpdateOrderCommandBuilder WithQuantity(int value)
    {
        _quantity = value;
        return this;
    }

    public UpdateOrderCommand Build() => new(_id, _productName, _quantity);
}