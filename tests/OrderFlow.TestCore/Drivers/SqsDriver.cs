using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace OrderFlow.TestCore.Drivers;

public class SqsDriver(IAmazonSQS sqs)
{
    public IAmazonSQS SqsClient => sqs;
    
    public async Task<string> CreateQueueAsync(string queueName, CancellationToken cancellationToken = default)
    {
        var createQueueResponse = await sqs.CreateQueueAsync(queueName, cancellationToken);
        return createQueueResponse.QueueUrl;
    }
    
    public async Task<IReadOnlyCollection<Message>?> ReadAllAsync(string queueUrl, CancellationToken cancellationToken = default)
    {
        var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 10,
            VisibilityTimeout = 60,
            MessageAttributeNames = ["*"]
        }, cancellationToken);

        return response?.Messages?.AsReadOnly();
    }

    public async Task ClearAsync(string queueUrl, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var response = await sqs.ReceiveMessageAsync(
                new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10
                }, cancellationToken);

            if (response.Messages.Count == 0)
                break;

            foreach (var message in response.Messages)
            {
                await sqs.DeleteMessageAsync(
                    queueUrl,
                    message.ReceiptHandle,
                    cancellationToken);
            }
        }
    }

    public async Task DeleteAsync(string queueUrl, CancellationToken cancellationToken = default)
    {
        await sqs.DeleteQueueAsync(queueUrl, cancellationToken);
    }
    
    public async Task<string> SendMessageAsync<TMessage>(
        TMessage message,
        string queueUrl,
        string? correlationId,
        CancellationToken cancellationToken = default)
    where TMessage : class
    {
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message)
        };

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "CorrelationId",
                    new MessageAttributeValue
                    {
                        StringValue = correlationId,
                        DataType = "String"
                    }
                }
            };
        }

        var response = await sqs.SendMessageAsync(request, cancellationToken);
        return response.MessageId;
    }
}