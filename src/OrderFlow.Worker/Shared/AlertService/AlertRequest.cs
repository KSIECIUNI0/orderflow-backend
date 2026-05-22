namespace OrderFlow.Worker.Shared.AlertService;

public record AlertRequest
{
    public required string Type { get; init; }
    public required string MessageId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Error { get; init; }
}