using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain.Entities;

public class OrderTests
{
    private const int MaxProductNameLength = 100;
    
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        const int quantity = 1;
        var productName = string.Empty;
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("productName");
    }
    
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        const int quantity = 1;
        var productName = new string('x', MaxProductNameLength + 50);
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("productName");
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Ctor_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int quantity)
    {
        var productName = new string('x', MaxProductNameLength);
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }
    
    [Fact]
    public void Ctor_ShouldCreateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        const string productName = "product name";
        const int quantity = 1;
        
        var result = new Order(productName, quantity);

        result.Id.Should().NotBeEmpty();
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        const int newQuantity = 2;
        var newProductName = string.Empty;
        
        var act = () => Sut().Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("productName");
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        const int newQuantity = 2;
        var newProductName = new string('x', MaxProductNameLength + 50);
        
        var act = () => Sut().Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("productName");
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Update_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int newQuantity)
    {
        var newProductName = new string('y', MaxProductNameLength);
        
        var act = () => Sut().Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public void Update_ShouldUpdateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        var sut = Sut();
        var expectedId = sut.Id;
        const string newProductName = "new product name";
        const int newQuantity = 2;
        
        sut.Update(newProductName, newQuantity);
        
        sut.Id.Should().Be(expectedId);
        sut.ProductName.Should().Be(newProductName);
        sut.Quantity.Should().Be(newQuantity);
    }

    private static Order Sut() => new("product name", 1);
}