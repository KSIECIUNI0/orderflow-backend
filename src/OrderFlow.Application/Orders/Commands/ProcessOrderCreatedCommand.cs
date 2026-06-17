using MediatR;
using OrderFlow.Application.Correlation;

namespace OrderFlow.Application.Orders.Commands;

public record ProcessOrderCreatedCommand(Guid OrderId) : IRequest, ICorrelated
{
    public string? CorrelationId { get; set; }
}