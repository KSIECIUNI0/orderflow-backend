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
        // Justification: Explicitly passing the parameter name is intentional here.
        // The built-in ThrowIfGreaterThan API does not use CallerArgumentExpression,
        // and relying on its automatic caller info would produce misleading values
        // such as "productName.Length". Supplying nameof(productName) ensures the
        // exception message contains the correct parameter name.
        #pragma warning disable S3236
        ArgumentOutOfRangeException.ThrowIfGreaterThan(productName.Length, 100, nameof(productName));
        #pragma warning restore S3236
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
    }
}