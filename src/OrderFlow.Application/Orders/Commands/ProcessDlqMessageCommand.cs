using MediatR;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Messaging;

namespace OrderFlow.Application.Orders.Commands;

public record ProcessDlqMessageCommand(
    QueueMessage QueueMessage,
    string EventType,
    string QueueName) : IRequest, ICorrelated
{
    public string? CorrelationId { get; set; }
}