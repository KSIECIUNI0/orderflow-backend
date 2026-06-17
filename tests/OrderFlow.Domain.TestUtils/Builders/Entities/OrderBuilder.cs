using OrderFlow.Domain.Entities;

namespace OrderFlow.Domain.TestUtils.Builders.Entities;

public class OrderBuilder
{
    private const int MaxProductNameLength = 100;
    private const string ValidProductName = "Laptop";
    
    private string _productName = ValidProductName;
    private int _quantity = 1;
    private Guid? _id;
    
    public static string TooLongProductName() => new('x', MaxProductNameLength + 1000);

    public OrderBuilder WithProductName(string value)
    {
        _productName = value;
        return this;
    }
    
    public OrderBuilder WithTooLongProductName()
    {
        _productName = TooLongProductName();
        return this;
    }
    
    public OrderBuilder WithEmptyProductName()
    {
        _productName = string.Empty;
        return this;
    }

    public OrderBuilder WithQuantity(int value)
    {
        _quantity = value;
        return this;
    }

    public OrderBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }

    public Order Build()
    {
        var order = new Order(
            _productName,
            _quantity);

        if (_id.HasValue)
        {
            typeof(Order)
                .GetProperty(nameof(Order.Id))!
                .SetValue(order, _id.Value);
        }
        
        return order;
    }
}