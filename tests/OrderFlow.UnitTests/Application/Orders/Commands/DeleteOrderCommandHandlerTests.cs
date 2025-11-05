using FluentAssertions;
using Moq;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Orders.Commands;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new DeleteOrderCommand(orderId);

        _orderRepository
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        var result = await Sut().Handle(command, CancellationToken.None);
        
        _orderRepository.Verify(
            or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _orderRepository.Verify(
            or => or.Delete(It.IsAny<Order>()),
            Times.Never);
        _unitOfWork.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnTrueAndDeleteOrder_WhenOrderExistsInDb()
    {
        const string productName = "Product";
        const int quantity = 1;
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new DeleteOrderCommand(orderId);
        
        _orderRepository
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var order = new Order(productName, quantity);
                typeof(Order).GetProperty("Id")!.SetValue(order, id);
                return order;
            });
        
        var result = await Sut().Handle(command, CancellationToken.None);
        
        _orderRepository.Verify(
            or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _orderRepository.Verify(
            or => or.Delete(It.Is<Order>(o => 
                o.Id == orderId 
                && o.ProductName == productName 
                && o.Quantity == quantity)),
            Times.Once);
        _unitOfWork.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDbThrowAnException()
    {
        const string dbErrorMessage = "DB failed";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var command = new DeleteOrderCommand(orderId);

        _orderRepository
            .Setup(or => or.GetByIdAsync(
                It.Is<Guid>(id => id == orderId),
                It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException(dbErrorMessage));
        
        var act = () =>  Sut().Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(dbErrorMessage);
    }
    
    private DeleteOrderCommandHandler Sut() 
        => new(_orderRepository.Object, _unitOfWork.Object);
}