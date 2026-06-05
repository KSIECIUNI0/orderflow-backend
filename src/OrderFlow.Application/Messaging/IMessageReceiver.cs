namespace OrderFlow.Application.Messaging;

public interface IMessageReceiver
{
    Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string queueUrl, CancellationToken cancellationToken);
    Task DeleteAsync(string queueUrl, string receiptHandle, CancellationToken cancellationToken);
}