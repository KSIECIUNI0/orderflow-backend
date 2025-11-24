using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using OrderFlow.Application.Common.EventBus;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Infrastructure.EventBus;

public class SqsEventBus : IEventBus
{
    private readonly IAmazonSQS _sqs;
    private readonly string _orderCreatedQueueUrl;

    public SqsEventBus(IAmazonSQS sqs, IConfiguration config)
    {
        _sqs = sqs;

        _orderCreatedQueueUrl = config["Sqs:OrderCreatedQueueUrl"]
                                ?? throw new ArgumentNullException(paramName: nameof(config),
                                    message: "Sqs:OrderCreatedQueueUrl not configured");
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent @event, string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event);

        var request = new SendMessageRequest
        {
            QueueUrl = _orderCreatedQueueUrl,
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

        await _sqs.SendMessageAsync(request, cancellationToken);
    }
}