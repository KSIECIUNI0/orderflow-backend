namespace OrderFlow.Application.Failures;

public interface IFailureStore
{
    Task SaveFailedMessageAsync(FailedMessage message);
    Task<List<FailedMessage>> GetAllAsync(string queueName);
    Task RemoveAsync(string queueName, string messageId);
}