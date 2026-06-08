using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Testing.Builders.Entities;
using OrderFlow.Testing.Builders.Handlers;

namespace OrderFlow.Application.Tests.Orders.Queries;

public class GetAllOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersFound()
    {
        // Arrange
        var query = new GetAllOrdersQuery();
        var builder = new GetAllOrdersQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        
        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);
        
        var sut = builder.Build();
        
        // Act
        var result = await sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handler_ShouldReturnListOfOrderResponse_WhenOrdersExist()
    {
        // Arrange
        var firstOrder = new OrderBuilder()
            .WithProductName("productName1")
            .WithQuantity(1)
            .Build();
        var secondOrder = new OrderBuilder()
            .WithProductName("productName2")
            .WithQuantity(2)
            .Build();
        var query = new GetAllOrdersQuery();
        var builder = new GetAllOrdersQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();
        
        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([firstOrder, secondOrder]);
        
        var sut = builder.Build();
        
        // Act
        var result = await sut.Handle(query, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should()
            .NotBeEmpty()
            .And.HaveCount(2)
            .And.AllBeOfType<OrderResponse>();
        
        result.Should()
            .SatisfyRespectively( first =>
                {
                    first.Id.Should().Be(firstOrder.Id);
                    first.ProductName.Should().Be(firstOrder.ProductName);
                    first.Quantity.Should().Be(firstOrder.Quantity);
                },
                second =>
                {
                    second.Id.Should().Be(secondOrder.Id);
                    second.ProductName.Should().Be(secondOrder.ProductName);
                    second.Quantity.Should().Be(secondOrder.Quantity);
                });

        await repository
            .Received(1)
            .GetAllAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        // Arrange
        const string dbErrorMessage = "DB failed";
        var query = new GetAllOrdersQuery();
        var builder = new GetAllOrdersQueryHandlerBuilder();
        var repository = builder.FreezeOrderRepository();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(query, TestContext.Current.CancellationToken);

        //Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
}