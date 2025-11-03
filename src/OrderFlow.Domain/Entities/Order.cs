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
        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;

        ValidateInvariants();
    }

    public void Update(string productName, int quantity)
    {
        ProductName = productName;
        Quantity = quantity;

        ValidateInvariants();
    }
    
    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(ProductName))
            throw new ArgumentException("Product name cannot be empty");
        
        if (ProductName.Length > 100)
            throw new ArgumentException("Product name is too long");
        
        if (Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");
    }
}