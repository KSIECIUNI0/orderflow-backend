using OrderFlow.Application.Failures.DTOs;

namespace OrderFlow.Application.Failures;

public interface IFailureStore
{
    Task SaveFailedMessageAsync(FailedMessage message, CancellationToken cancellationToken = default);
    Task<FailedMessagePage> GetPageAsync(int pageSize, string? continuationToken, CancellationToken cancellationToken = default);
    Task RemoveAsync(string queueName, string messageId, CancellationToken cancellationToken = default);
}