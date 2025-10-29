using MediatR;

namespace OrderFlow.Application.Orders.Commands;

public record DeleteOrderCommand(Guid Id) : IRequest<bool>;