namespace OrderFlow.Worker.Shared.FailureStore;

public interface IFailureStore
{
    Task SaveFailedMessageAsync(FailedMessage message);
    Task<List<FailedMessage>> GetAllAsync(string queueName);
    Task RemoveAsync(string queueName, string messageId);
}