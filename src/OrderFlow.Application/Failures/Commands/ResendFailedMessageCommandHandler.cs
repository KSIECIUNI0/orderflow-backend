using MediatR;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Application.Failures.Commands;

public class ResendFailedMessageCommandHandler(
    IFailureStore store,
    IMessagePublisher messagePublisher) : IRequestHandler<ResendFailedMessageCommand>
{
    public async Task Handle(
        ResendFailedMessageCommand command,
        CancellationToken cancellationToken)
    {
        var message = await store.GetFailedMessageAsync(command.QueueIdentifier, command.MessageId, cancellationToken);
        if (message is null)
        {
            throw new FailedMessageNotFoundException(command.MessageId);
        }

        var outgoingMessage = new OutgoingMessage
        {
            QueueIdentifier = command.QueueIdentifier,
            Body = message.Body,
            CorrelationId = message.CorrelationId
        };
        await messagePublisher.PublishAsync(outgoingMessage, cancellationToken);
        
        await store.DeleteFailedMessageAsync(command.QueueIdentifier, command.MessageId, cancellationToken);
    }
}