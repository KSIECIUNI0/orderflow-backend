namespace OrderFlow.Application.Messaging;

public interface IMessageReceiver
{
    Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string queueUrl, CancellationToken cancellationToken);
    Task CompleteAsync(QueueMessage message, CancellationToken cancellationToken);
}