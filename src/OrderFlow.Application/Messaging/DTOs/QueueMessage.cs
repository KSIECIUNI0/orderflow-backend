namespace OrderFlow.Application.Messaging.DTOs;

public sealed record QueueMessage
{
    public required string MessageId { get; init; }
    public required string Body { get; init; }
    public required string CorrelationId { get; init; }
    public required string ReceiptHandle { get; init; }
    public required string QueueUrl { get; init; }
}