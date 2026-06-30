using OrderFlow.Application.Failures.DTOs;

namespace OrderFlow.Application.Failures;

public interface IFailureStore
{
    Task SaveAsync(FailedMessage message, CancellationToken cancellationToken = default);
    Task<FailedMessagePage> GetPageAsync(int pageSize, string? continuationToken, CancellationToken cancellationToken = default);
    Task<FailedMessage?> GetFailedMessageAsync(string queueIdentifier, string messageId, CancellationToken cancellationToken = default);
    Task DeleteFailedMessageAsync(string queueIdentifier, string messageId, CancellationToken cancellationToken = default);
}