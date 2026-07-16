using MediatR;
using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Application.Orders.Commands;

public class DeleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOrderCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return false;

        orderRepository.Delete(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}