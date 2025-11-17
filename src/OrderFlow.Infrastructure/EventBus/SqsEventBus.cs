using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using OrderFlow.Application.Common.EventBus;

namespace OrderFlow.Infrastructure.EventBus;

public class SqsEventBus : IEventBus
{
    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;

    public SqsEventBus(IAmazonSQS sqs, IConfiguration config)
    {
        _sqs = sqs;
        
        _queueUrl = config["Sqs:OrderCreatedQueue"] 
                    ?? throw new ArgumentNullException(paramName: nameof(config), message: "Sqs:QueueUrl not configured");
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(@event);

        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = json
        };
        
        await _sqs.SendMessageAsync(request, cancellationToken);
    }
}