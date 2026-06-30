using OrderFlow.Application.Messaging.DTOs;

namespace OrderFlow.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken = default);
}