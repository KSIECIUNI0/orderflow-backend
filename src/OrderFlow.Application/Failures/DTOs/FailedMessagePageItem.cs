namespace OrderFlow.Application.Failures.DTOs;

public record FailedMessagePageItem(
    string MessageId,
    string QueueIdentifier,
    string? CorrelationId,
    string? EventType,
    DateTimeOffset FailedAt);