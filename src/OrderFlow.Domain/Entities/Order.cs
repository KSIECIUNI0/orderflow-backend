namespace OrderFlow.Domain.Entities;

public class Order
{
    private Order() {} // EF Core
    
    public Guid Id { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    public Order(
        string productName,
        int quantity)
    {
        ValidateInvariants(productName, quantity);
        
        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;
    }

    public void Update(string productName, int quantity)
    {
        ValidateInvariants(productName, quantity);
        
        ProductName = productName;
        Quantity = quantity;
    }
    
    private static void ValidateInvariants(
        string productName,
        int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(productName.Length, 100, nameof(productName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
    }
}