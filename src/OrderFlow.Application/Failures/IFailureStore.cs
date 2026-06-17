namespace OrderFlow.Application.Failures;

public interface IFailureStore
{
    Task SaveFailedMessageAsync(FailedMessage message, CancellationToken cancellationToken = default);
    Task<List<FailedMessage>> GetAllAsync(string queueName, CancellationToken cancellationToken = default);
    Task RemoveAsync(string queueName, string messageId, CancellationToken cancellationToken = default);
}