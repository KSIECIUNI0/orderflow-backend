using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.EventBus;

public class SqsEventBus(
    IAmazonSQS sqs,
    IOptions<OrderCreatedQueueOptions> options) : IEventBus
{
    private readonly OrderCreatedQueueOptions _options = options.Value;

    public async Task PublishOrderCreatedAsync(
        OrderCreatedEvent @event,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event);

        var request = new SendMessageRequest
        {
            QueueUrl = _options.Url,
            MessageBody = json,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "CorrelationId", 
                    new MessageAttributeValue
                    {
                        StringValue = correlationId,
                        DataType = "String"
                    }
                }
            }
        };

        await sqs.SendMessageAsync(request, cancellationToken);
    }
}