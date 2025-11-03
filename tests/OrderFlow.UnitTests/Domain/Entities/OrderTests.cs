using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        const int quantity = 1;
        const string expectedErrorMessage = "Product name cannot be empty";
        var productName = string.Empty;
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        const int quantity = 1;
        const string expectedErrorMessage = "Product name is too long";
        var productName = new string('x', 150);
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Ctor_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int quantity)
    {
        const string productName = "product name";
        const string expectedErrorMessage = "Quantity must be greater than zero";
        
        var act = () => new Order(productName, quantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Fact]
    public void Ctor_ShouldCreateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        const string productName = "product name";
        const int quantity = 1;
        
        var result = new Order(productName, quantity);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        var sut = CreateSut();
        
        const int newQuantity = 2;
        const string expectedErrorMessage = "Product name cannot be empty";
        var newProductName = string.Empty;
        
        var act = () => sut.Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        var sut = CreateSut();
        
        const int newQuantity = 2;
        const string expectedErrorMessage = "Product name is too long";
        var newProductName = new string('x', 150);
        
        var act = () => sut.Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Update_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int newQuantity)
    {
        var sut = CreateSut();
        
        const string expectedErrorMessage = "Quantity must be greater than zero";
        const string newProductName = "new product name";
        
        var act = () => sut.Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentException>()
            .WithMessage(expectedErrorMessage);
    }

    [Fact]
    public void Update_ShouldUpdateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        var sut = CreateSut();
        var expectedId = sut.Id;
        const string newProductName = "new product name";
        const int newQuantity = 2;
        
        sut.Update(newProductName, newQuantity);
        
        sut.Id.Should().Be(expectedId);
        sut.ProductName.Should().Be(newProductName);
        sut.Quantity.Should().Be(newQuantity);
    }

    private static Order CreateSut()
        => new Order("product name", 1);
}