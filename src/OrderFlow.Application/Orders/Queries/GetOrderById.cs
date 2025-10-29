using MediatR;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries;

public record GetOrderById(Guid Id) : IRequest<OrderResponse?>;