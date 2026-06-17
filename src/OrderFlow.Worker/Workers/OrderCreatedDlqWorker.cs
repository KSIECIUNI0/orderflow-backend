using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Worker.Processors;

namespace OrderFlow.Worker.Workers;

public class OrderCreatedDlqWorker(
    IMessageReceiver messageReceiver,
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
            
            if (messages.Count == 0)
                continue;
            
            using var scope = serviceScopeFactory.CreateScope();
            var messageProcessor = scope.ServiceProvider.GetRequiredService<OrderCreatedDlqMessageProcessor>();

            foreach (var message in messages)
            {
                await messageProcessor.ProcessAsync(message, stoppingToken);
            }
        }
    }
}