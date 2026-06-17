using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Application.TestUtils.Builders.Commands;

public class CreateOrderCommandBuilder
{
    private const string ValidProductName = "Laptop";
    
    private string _productName = ValidProductName;
    private int _quantity = 1;
    
    public CreateOrderCommandBuilder WithProductName(string value)
    {
        _productName = value;
        return this;
    }
    
    public CreateOrderCommandBuilder WithEmptyProductName()
    {
        _productName = string.Empty;
        return this;
    }
    
    public CreateOrderCommandBuilder WithQuantity(int value)
    {
        _quantity = value;
        return this;
    }

    public CreateOrderCommand Build() => new(_productName, _quantity);
}