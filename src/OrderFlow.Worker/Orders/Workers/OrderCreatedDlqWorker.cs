using Amazon.SQS;
using Amazon.SQS.Model;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Worker.Shared.AlertService;
using OrderFlow.Worker.Shared.FailureStore;
using Serilog.Context;

namespace OrderFlow.Worker.Orders.Workers;

public class OrderCreatedDlqWorker : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly IFailureStore _failureStore;
    private readonly IAlertService _alertService;
    private readonly ILogger<OrderCreatedDlqWorker> _logger;
    
    private readonly string _queueName;
    private readonly string _dlqQueueUrl;
    private readonly int _maxNumberOfMessages;
    private readonly int _waitTimeSeconds;

    public OrderCreatedDlqWorker(
        IAmazonSQS sqs,
        IFailureStore failureStore,
        IAlertService alertService,
        IConfiguration configuration,
        ILogger<OrderCreatedDlqWorker> logger)
    {
        _sqs = sqs;
        _logger = logger;
        _failureStore = failureStore;
        _alertService = alertService;
        
        var sqsConfig = configuration.GetSection("Sqs");
        _queueName = sqsConfig.GetValue<string>("OrderCreatedQueueName")
                       ?? throw new ArgumentNullException(paramName: nameof(configuration), message: "Sqs:OrderCreatedQueueName not configured");
        _dlqQueueUrl = sqsConfig.GetValue<string>("OrderCreatedDlqQueueUrl")
                       ?? throw new ArgumentNullException(paramName: nameof(configuration), message: "Sqs:OrderCreatedDlqQueueUrl not configured");
        _maxNumberOfMessages = sqsConfig.GetValue<int>("MaxNumberOfMessages");
        _waitTimeSeconds = sqsConfig.GetValue<int>("WaitTimeSeconds");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderCreatedDlqWorker listening on {Url}", _dlqQueueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _dlqQueueUrl,
                MaxNumberOfMessages = _maxNumberOfMessages,
                WaitTimeSeconds = _waitTimeSeconds,
                MessageAttributeNames = ["*"]
            }, stoppingToken);
            
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
            var corrId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
                ? attr.StringValue
                : "UNKNOWN";
            using (LogContext.PushProperty("CorrelationId", corrId))
            {
                _logger.LogError(
                    "DLQ message received. CorrelationId={CorrelationId}, MessageId={MessageId}, Body={Body}",
                    corrId,
                    message.MessageId,
                    message.Body);

                await SaveFailedMessageAsync(message, corrId);
                await SendAlertAsync(message, corrId);

                await _sqs.DeleteMessageAsync(_dlqQueueUrl, message.ReceiptHandle, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "OrderCreatedDlqWorker failed to process a message");
        }
    }

    private async Task SaveFailedMessageAsync(Message message, string corrId)
    {
        _logger.LogInformation("Save failed message to S3");
        var failedMessage = new FailedMessage
        {
            MessageId = message.MessageId,
            QueueName = _queueName,
            EventType = nameof(OrderCreatedEvent),
            CorrelationId = corrId,
            Body = message.Body
        };
        
        await _failureStore.SaveFailedMessageAsync(failedMessage);
    }

    private async Task SendAlertAsync(Message message, string corrId)
    {
        _logger.LogInformation("Send alert request into AlertService");
        var request = new AlertRequest
        {
            MessageId = message.MessageId,
            CorrelationId = corrId,
            Type = "ERROR",
            Error = "The message reached DLQ"
        };
            
        await _alertService.SendAsync(request);
    }
}