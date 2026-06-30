using Amazon.SQS;
using Amazon.SQS.Model;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Messaging.Identifiers;

namespace OrderFlow.Infrastructure.Messaging;

public class SqsMessagePublisher(
    IAmazonSQS sqs,
    IQueueIdentifierResolver queueIdentifierResolver) : IMessagePublisher
{
    public async Task PublishAsync(
        OutgoingMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var queueUrl = queueIdentifierResolver.ResolveQueueUrl(message.QueueIdentifier);
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = message.Body
        };

        if (!string.IsNullOrEmpty(message.CorrelationId))
        {
            request.MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "CorrelationId",
                    new MessageAttributeValue
                    {
                        StringValue = message.CorrelationId,
                        DataType = "String"
                    }
                }
            };
        }

        await sqs.SendMessageAsync(request, cancellationToken);
    }
}