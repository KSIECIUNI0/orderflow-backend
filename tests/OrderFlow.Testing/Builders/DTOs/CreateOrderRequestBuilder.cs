using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Testing.Builders.DTOs;

public class CreateOrderRequestBuilder
{
    private const int MaxProductNameLength = 100;
    private const string ValidProductName = "Laptop";

    private string _productName = ValidProductName;
    private int _quantity = 1;
    
    private static string TooLongProductName() => new('x', MaxProductNameLength + 1000);
    
    public CreateOrderRequestBuilder WithTooLongProductName()
    {
        _productName = TooLongProductName();
        return this;
    }
    
    public CreateOrderRequestBuilder WithEmptyProductName()
    {
        _productName = string.Empty;
        return this;
    }
    
    public CreateOrderRequestBuilder WithNegativeQuantity()
    {
        _quantity = -100;
        return this;
    }
    
    public CreateOrderRequest Build()
    {
        return new CreateOrderRequest(_productName, _quantity);
    }
}