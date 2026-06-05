using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.Configurations;
using Serilog.Context;

namespace OrderFlow.Worker.Orders.Workers;

public class OrderCreatedWorker(
    IMessageReceiver messageReceiver,
    IOptions<OrderCreatedQueueOptions> orderCreatedQueueOptions,
    ILogger<OrderCreatedWorker> logger,
    IServiceScopeFactory serviceScopeFactory,
    ICorrelationIdAccessor correlationIdAccessor)
    : BackgroundService
{
    private readonly OrderCreatedQueueOptions  _orderCreatedQueueOptions = orderCreatedQueueOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OrderCreatedWorker listening on {Url}", _orderCreatedQueueOptions.Url);

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await messageReceiver.ReceiveAsync(
                _orderCreatedQueueOptions.Url,
                stoppingToken);
            
            if (messages.Count == 0)
                continue;
            
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
                logger.LogInformation("Received message: {Body}", message.Body);
                var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
                ArgumentNullException.ThrowIfNull(@event);

                logger.LogInformation("OrderCreatedEvent received for {OrderId}", @event.OrderId);
                
                var scope = serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                var command = new ProcessOrderCreatedCommand(@event.OrderId);
                await mediator.Send(command, stoppingToken);

                await messageReceiver.DeleteAsync(_orderCreatedQueueOptions.Url, message.ReceiptHandle, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while processing message: {MessageId} - CorrelationId: {CorrelationId}", message.MessageId, correlationIdAccessor.CorrelationId);
        }
    }
}