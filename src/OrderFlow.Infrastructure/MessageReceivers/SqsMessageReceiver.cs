using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.MessageReceivers;

public class SqsMessageReceiver(
    IAmazonSQS sqs,
    IOptions<SqsOptions> options) : IMessageReceiver
{
    private readonly SqsOptions _options = options.Value;

    public async Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(
        string queueUrl,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(queueUrl, nameof(queueUrl));
        
        var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = _options.MaxNumberOfMessages,
            WaitTimeSeconds = _options.WaitTimeSeconds,
            VisibilityTimeout = _options.VisibilityTimeout,
            MessageAttributeNames = ["*"]
        }, cancellationToken);

        return response?.Messages?.Select(ToQueueMessage).ToList() ?? [];
    }

    public async Task DeleteAsync(
        string queueUrl,
        string receiptHandle,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(queueUrl, nameof(queueUrl));
        ArgumentException.ThrowIfNullOrEmpty(receiptHandle, nameof(receiptHandle));
        
        await sqs.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
    }

    private static QueueMessage ToQueueMessage(Message message)
    {
        var correlationId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
            ? attr.StringValue
            : "UNKNOWN";

        return new QueueMessage
        {
            CorrelationId = correlationId,
            Body = message.Body,
            MessageId = message.MessageId,
            ReceiptHandle = message.ReceiptHandle,
        };
    }
}