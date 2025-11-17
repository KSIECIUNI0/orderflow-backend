using FluentAssertions;
using Moq;
using OrderFlow.Application.Common.EventBus;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();

    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WhenPassProductNameAndQuantity()
    {
        const string productName = "Product";
        const int quantity = 1;
        var savedOrderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new CreateOrderCommand(productName, quantity);

        _orderRepositoryMock
            .Setup(repo => repo.AddAsync(
                It.Is<Order>(o =>
                    o.ProductName == productName && o.Quantity == quantity),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) =>
            {
                typeof(Order).GetProperty("Id")!.SetValue(o, savedOrderId);
                return o;
            });

        var result = await Sut().Handle(command, CancellationToken.None);

        _orderRepositoryMock.Verify(
            or => or.AddAsync(
                It.Is<Order>(o =>
                    o.ProductName == productName && o.Quantity == quantity),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        result.Should().NotBeNull();
        result.Id.Should().Be(savedOrderId);
        result.ProductName.Should().Be(productName);
        result.Quantity.Should().Be(quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowAnArgumentException_WhenDomainValidationFails()
    {
        var command = new CreateOrderCommand(string.Empty, 1);
        
        var act = () => Sut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    private CreateOrderCommandHandler Sut() =>
        new (_orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _eventBusMock.Object);
}