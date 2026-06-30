using System.Text.Json;
using MediatR;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Events;
using Serilog.Context;

namespace OrderFlow.Worker.Processors;

public class OrderCreatedMessageProcessor(
    ICorrelationIdAccessor correlationIdAccessor,
    IMessageReceiver messageReceiver,
    IMediator mediator,
    ILogger<OrderCreatedMessageProcessor> logger)
{
    public async Task ProcessAsync(QueueMessage message, CancellationToken stoppingToken)
    {
        try
        {
            correlationIdAccessor.Set(message.CorrelationId);
            using (LogContext.PushProperty("CorrelationId", correlationIdAccessor.CorrelationId))
            {
                logger.LogInformation("Received message: {Body}", message.Body);
                var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
                ArgumentNullException.ThrowIfNull(@event, nameof(@event));
                @event.Validate();

                logger.LogInformation("OrderCreatedEvent received for {OrderId}", @event.OrderId);
                
                var command = new ProcessOrderCreatedCommand(@event.OrderId);
                await mediator.Send(command, stoppingToken);

                await messageReceiver.CompleteAsync(message, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while processing message: {MessageId} - CorrelationId: {CorrelationId}", message.MessageId, correlationIdAccessor.CorrelationId);
        }
    }
}