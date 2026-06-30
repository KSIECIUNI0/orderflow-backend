using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.Messaging;

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

        return response?.Messages?.Select(m => ToQueueMessage(m, queueUrl)).ToList() ?? [];
    }

    public async Task CompleteAsync(
        QueueMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        
        await sqs.DeleteMessageAsync(message.QueueUrl, message.ReceiptHandle, cancellationToken);
    }

    private static QueueMessage ToQueueMessage(Message message, string queueUrl)
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
            QueueUrl = queueUrl
        };
    }
}