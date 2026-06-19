namespace OrderFlow.Application.Failures;

public record FailedMessagePageItem(
    string MessageId,
    string QueueIdentifier,
    string? CorrelationId,
    string? EventType,
    DateTimeOffset FailedAt);