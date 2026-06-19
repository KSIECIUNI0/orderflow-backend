namespace OrderFlow.Application.Failures.DTOs;

public record FailedMessagePage(
    IReadOnlyList<FailedMessagePageItem> Items,
    bool IsTruncated,
    string? ContinuationToken);