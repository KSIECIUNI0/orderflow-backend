using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Domain.Entities;
using OrderFlow.Testing.Builders.Commands;
using OrderFlow.Testing.Builders.Entities;
using OrderFlow.Testing.Builders.Handlers;

namespace OrderFlow.Application.Tests.Orders.Commands;

public class DeleteOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new DeleteOrderCommandBuilder()
            .WithOrderId(orderId)
            .Build();
        var builder = new DeleteOrderCommandHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        var unitOfWork = builder.FreezeUnitOfWork();

        repository
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>())
            .Returns(null as Order);
        
        var sut = builder.Build();

        // Act
        var result = await sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await repository
            .Received(1)
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>());
        repository
            .DidNotReceive()
            .Delete(Arg.Any<Order>());

        await unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
        
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnTrueAndDeleteOrder_WhenOrderExistsInDb()
    {
        // Arrange
        const string productName = "Product";
        const int quantity = 1;
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var order = new OrderBuilder()
            .WithId(orderId)
            .WithProductName(productName)
            .Build();
        var command = new DeleteOrderCommandBuilder()
            .WithOrderId(orderId)
            .Build();
        var builder = new DeleteOrderCommandHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        var unitOfWork = builder.FreezeUnitOfWork();
        
        repository
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>())
            .Returns(order);

        var sut = builder.Build();
        
        // Act
        var result = await sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await repository
            .Received(1)
            .GetByIdAsync(
                Arg.Is<Guid>(id => id == orderId),
                Arg.Any<CancellationToken>());
        
        repository
            .Received(1)
            .Delete(Arg.Is<Order>(o =>
                o.Id == orderId
                && o.ProductName == productName
                && o.Quantity == quantity));
            
        await unitOfWork
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
        
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        // Arrange
        const string dbErrorMessage = "DB failed";
        var command = new DeleteOrderCommandBuilder().Build();
        var builder = new DeleteOrderCommandHandlerBuilder();
        var repository = builder.FreezeOrderRepository();

        repository
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var sut = builder.Build();
        
        // Act
        var act = () =>  sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
}