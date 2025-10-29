using MediatR;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands;

public record CreateOrderCommand(
    string ProductName,
    int Quantity)
    : IRequest<OrderResponse>;