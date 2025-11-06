using FluentAssertions;
using Moq;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Orders.Queries;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersFound()
    {
        var query = new GetAllOrdersQuery();
        
        _orderRepositoryMock
            .Setup(or => or.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        
        var result = await Sut().Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_ShouldReturnListOfOrderResponse_WhenOrdersExist()
    {
        var firstOrder = new Order("productName1", 1);
        var secondOrder = new Order("productName2", 2);
        var query = new GetAllOrdersQuery();
        
        _orderRepositoryMock
            .Setup(or => or.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstOrder, secondOrder]);
        
        var result = await Sut().Handle(query, CancellationToken.None);
        
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
        
        _orderRepositoryMock.Verify(
            or => or.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        const string dbErrorMessage = "DB failed";
        var query = new GetAllOrdersQuery();

        _orderRepositoryMock
            .Setup(or => or.GetAllAsync(It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var act = () =>  Sut().Handle(query, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
    
    private GetAllOrdersQueryHandler Sut() => new(_orderRepositoryMock.Object);
}