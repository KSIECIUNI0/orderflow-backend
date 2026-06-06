using FluentAssertions;
using NSubstitute;
using OrderFlow.Domain.Entities;
using OrderFlow.Testing.Builders;
using OrderFlow.Testing.Builders.Commands;
using OrderFlow.Testing.Builders.Entities;
using OrderFlow.Testing.Builders.Handlers;

namespace OrderFlow.Application.Tests.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WhenPassProductNameAndQuantity()
    {
        // Arrange
        const string productName = "Product";
        const int quantity = 1;
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var order = new OrderBuilder()
            .WithProductName(productName)
            .WithQuantity(quantity)
            .WithId(orderId)
            .Build();
        var command = new CreateOrderCommandBuilder()
            .WithProductName(productName)
            .WithQuantity(quantity)
            .Build();

        var builder = new CreateOrderCommandHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        var unitOfWork = builder.FreezeUnitOfWork();

        repository
            .AddAsync(
                Arg.Is<Order>(o => o.ProductName == productName && o.Quantity == quantity),
                Arg.Any<CancellationToken>())
            .Returns(order);
        
        var sut = builder.Build();

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        await repository
            .Received(1)
            .AddAsync(Arg.Is<Order>(o => o.ProductName == productName && o.Quantity == quantity),
                Arg.Any<CancellationToken>());
        await unitOfWork
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
        
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnArgumentException_WhenDomainValidationFails()
    {
        // Arrange
        var command = new CreateOrderCommandBuilder()
            .WithEmptyProductName()
            .Build();
        var sut = new CreateOrderCommandHandlerBuilder().Build();
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}