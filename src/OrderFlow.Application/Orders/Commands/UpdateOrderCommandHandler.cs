using MediatR;
using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Application.Orders.Commands;

public class UpdateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOrderCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return false;

        order.Update(request.ProductName, request.Quantity);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}