using MediatR;

namespace OrderFlow.Application.Failures.Commands;

public record ResendFailedMessageCommand(
    string QueueIdentifier,
    string MessageId) : IRequest
{
}