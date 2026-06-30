namespace OrderFlow.Application.Messaging.DTOs;

public record OutgoingMessage
{
    public required string QueueIdentifier { get; init; }
    public required string Body { get; init; }
    public string? CorrelationId { get; init; }
}