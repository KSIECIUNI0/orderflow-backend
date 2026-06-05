using MediatR;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Infrastructure.Configurations;
using Serilog.Context;

namespace OrderFlow.Worker.Orders.Workers;

public class OrderCreatedDlqWorker(
    IMessageReceiver messageReceiver,
    ICorrelationIdAccessor correlationIdAccessor,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OrderCreatedDlqOptions> orderCreatedDlqOptions,
    ILogger<OrderCreatedDlqWorker> logger)
    : BackgroundService
{
    private readonly OrderCreatedDlqOptions _orderCreatedDlqOptions = orderCreatedDlqOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OrderCreatedDlqWorker listening on {Url}", _orderCreatedDlqOptions.Url);

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await messageReceiver.ReceiveAsync(
                _orderCreatedDlqOptions.Url,
                stoppingToken);
            
            foreach (var message in messages)
            {
                correlationIdAccessor.Set(message.CorrelationId);
                await ProcessMessage(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessage(QueueMessage message, CancellationToken stoppingToken)
    {
        try
        {
            using (LogContext.PushProperty("CorrelationId", correlationIdAccessor.CorrelationId))
            {
                logger.LogTrace("DLQ received message: {MessageId} - {Body}", message.MessageId, message.Body);
                
                var scope = serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                 
                var command = new ProcessDlqMessageCommand(message, _orderCreatedDlqOptions.OrderCreatedQueueName);
                await mediator.Send(command, stoppingToken);

                await messageReceiver.DeleteAsync(_orderCreatedDlqOptions.Url, message.ReceiptHandle, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "OrderCreatedDlqWorker failed to process a message");
        }
    }
}