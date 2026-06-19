namespace OrderFlow.Application.Failures;

public static class FailedMessageMapping
{
    public static FailedMessagePageItem ToPageItem(this FailedMessage message) =>
        new(
            message.MessageId,
            message.QueueIdentifier,
            message.CorrelationId,
            message.EventType,
            message.FailedAt);
}