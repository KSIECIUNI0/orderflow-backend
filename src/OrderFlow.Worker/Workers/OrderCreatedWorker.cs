using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Worker.Processors;

namespace OrderFlow.Worker.Workers;

public class OrderCreatedWorker(
    IMessageReceiver messageReceiver,
    IOptions<OrderCreatedQueueOptions> orderCreatedQueueOptions,
    ILogger<OrderCreatedWorker> logger,
    IServiceScopeFactory serviceScopeFactory)
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
            
            using var scope = serviceScopeFactory.CreateScope();
            var messageProcessor = scope.ServiceProvider.GetRequiredService<OrderCreatedMessageProcessor>();
            
            foreach (var message in messages)
            {
                await messageProcessor.ProcessAsync(message, stoppingToken);
            }
        }
    }
}