using FluentAssertions;
using Moq;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WhenPassProductNameAndQuantity()
    {
        const string productName = "Product";
        const int quantity = 1;
        var order = new Order(productName, quantity);
        var command = new CreateOrderCommand(productName, quantity);

        _orderRepository
            .Setup(repo => repo.AddAsync(
                It.Is<Order>(o =>
                    o.ProductName == productName && o.Quantity == quantity),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);
    }
    
    private CreateOrderCommandHandler Sut() =>
        new (_orderRepository.Object, _unitOfWork.Object);
}