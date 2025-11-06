using FluentAssertions;
using Moq;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Orders.Queries;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);
        
        var result = await Sut().Handle(query, CancellationToken.None);

        result.Should().BeNull();

        _orderRepositoryMock.Verify(
            or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WhenOrderExist()
    {
        const string productName = "product name";
        const int quantity = 1;
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetOrderByIdQuery(orderId);
        
        _orderRepositoryMock
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var order = new Order(productName, quantity);
                typeof(Order).GetProperty("Id")!.SetValue(order, id);
                return order;
            });
        
        var result = await Sut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);

        _orderRepositoryMock.Verify(
            or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        const string dbErrorMessage = "DB failed";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var act = () =>  Sut().Handle(query, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
    
    private GetOrderByIdQueryHandler Sut() => new(_orderRepositoryMock.Object);
}