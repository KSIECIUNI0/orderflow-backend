using MediatR;

namespace OrderFlow.Application.Orders.Commands;

public record UpdateOrderCommand(Guid Id, string ProductName, int Quantity) : IRequest<bool>;