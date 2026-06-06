using FluentAssertions;
using OrderFlow.Testing.Builders;
using OrderFlow.Testing.Builders.Entities;

namespace OrderFlow.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        const int quantity = 1;
        var builder = new OrderBuilder()
            .WithEmptyProductName()
            .WithQuantity(quantity);
        
        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithParameterName("productName");
    }
    
    [Fact]
    public void Ctor_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        const int quantity = 1;
        var builder = new OrderBuilder()
            .WithTooLongProductName()
            .WithQuantity(quantity);
        
        var act = () => builder.Build();

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("productName");
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Ctor_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int quantity)
    {
        var builder = new OrderBuilder()
            .WithQuantity(quantity);
        
        var act = () => builder.Build();

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }
    
    [Fact]
    public void Ctor_ShouldCreateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        const string productName = "product name";
        const int quantity = 1;
        
        var sut = new OrderBuilder()
            .WithProductName(productName)
            .WithQuantity(quantity)
            .Build();

        sut.Id.Should().NotBeEmpty();
        sut.ProductName.Should().Be(productName);
        sut.Quantity.Should().Be(quantity);
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsEmpty()
    {
        const int newQuantity = 2;
        var newProductName = string.Empty;
        var sut = new OrderBuilder().Build();
        
        var act = () => sut.Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("productName");
    }
    
    [Fact]
    public void Update_ShouldThrowAnArgumentException_WhenProductNameIsTooLong()
    {
        const int newQuantity = 2;

        var newTooLongProductName = OrderBuilder.TooLongProductName();
        var sut = new OrderBuilder().Build();
        
        var act = () => sut.Update(newTooLongProductName, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("productName");
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Update_ShouldThrowAnArgumentException_WhenQuantityIsNegativeOrZero(int newQuantity)
    {
        const string newProductName = "ValidProductName";
        var sut = new OrderBuilder().Build();
        
        var act = () => sut.Update(newProductName, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public void Update_ShouldUpdateAnInstance_WhenProductNameIsNotEmptyAndQuantityIsGreaterThanZero()
    {
        var sut = new OrderBuilder().Build();
        var expectedId = sut.Id;
        const string newProductName = "new product name";
        const int newQuantity = 2;
        
        sut.Update(newProductName, newQuantity);
        
        sut.Id.Should().Be(expectedId);
        sut.ProductName.Should().Be(newProductName);
        sut.Quantity.Should().Be(newQuantity);
    }
}