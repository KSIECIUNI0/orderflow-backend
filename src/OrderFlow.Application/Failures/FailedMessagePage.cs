namespace OrderFlow.Application.Failures;

public record FailedMessagePage(
    IReadOnlyList<FailedMessagePageItem> Items,
    bool IsTruncated,
    string? ContinuationToken);