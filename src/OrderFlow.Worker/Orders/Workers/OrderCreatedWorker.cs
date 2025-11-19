using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using MediatR;
using OrderFlow.Application.Orders.Events;

namespace OrderFlow.Worker.Orders.Workers;

public class OrderCreatedWorker : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly ILogger<OrderCreatedWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly string _queueUrl;
    private readonly int _maxNumberOfMessages;
    private readonly int _waitTimeSeconds;
    private readonly int _visibilityTimeout;

    public OrderCreatedWorker(
        IAmazonSQS sqs,
        IConfiguration configuration,
        ILogger<OrderCreatedWorker> logger,
        IServiceProvider serviceProvider)
    {
        _sqs = sqs;
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        var sqsConfig = configuration.GetSection("Sqs");
        _queueUrl = sqsConfig.GetValue<string>("OrderCreatedQueueUrl")
                    ?? throw new ArgumentNullException(paramName: nameof(configuration), message: "Sqs:OrderCreatedQueueUrl not configured");
        _maxNumberOfMessages = sqsConfig.GetValue<int>("MaxNumberOfMessages");
        _waitTimeSeconds = sqsConfig.GetValue<int>("WaitTimeSeconds");
        _visibilityTimeout = sqsConfig.GetValue<int>("VisibilityTimeout");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderCreatedWorker listening on {Url}", _queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = _maxNumberOfMessages,
                WaitTimeSeconds = _waitTimeSeconds,
                VisibilityTimeout = _visibilityTimeout
            }, stoppingToken);
            
            if (response.Messages.Count == 0)
                continue;
            
            foreach (var message in response.Messages)
            {
                await ProcessMessage(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessage(Message message, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Received message: {Body}", message.Body);
            var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
            ArgumentNullException.ThrowIfNull(@event);

            var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(@event, stoppingToken);

            await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while processing message {Id}", message.MessageId);
        }
    }
}