using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Domain.Entities;
using OrderFlow.Testing.Builders.Entities;
using OrderFlow.Testing.Builders.Handlers;
using OrderFlow.Testing.Builders.Queries;

namespace OrderFlow.Application.Tests.Orders.Queries;

public class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetOrderByIdQueryBuilder()
            .WithId(orderId)
            .Build();
        var builder = new GetOrderByIdQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();

        repository
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>())
            .Returns(null as Order);
        
        var sut = builder.Build();
        
        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await repository
            .Received(1)
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WhenOrderExist()
    {
        // Assert
        const string productName = "product name";
        const int quantity = 1;
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetOrderByIdQueryBuilder()
            .WithId(orderId)
            .Build();
        var order = new OrderBuilder()
            .WithId(orderId)
            .WithProductName(productName)
            .WithQuantity(quantity)
            .Build();
        var builder = new GetOrderByIdQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        
        repository
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>())
            .Returns(order);
        
        var sut = builder.Build();
        
        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);

        await repository
            .Received(1)
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        // Arrange
        const string dbErrorMessage = "DB failed";
        var query = new GetOrderByIdQueryBuilder().Build();
        var builder = new GetOrderByIdQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        
        repository
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var sut =  builder.Build();
        
        // Act
        var act = () => sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
}