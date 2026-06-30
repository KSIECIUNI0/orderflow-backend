namespace OrderFlow.Application.Failures.DTOs;

public record FailedMessage
{
    public required string MessageId { get; init; }
    public required string QueueIdentifier { get; init; }
    public string? CorrelationId { get; init; }
    public string? EventType { get; init; }
    public required string Body { get; init; }
    public DateTimeOffset FailedAt { get; init; } = DateTimeOffset.UtcNow;
}