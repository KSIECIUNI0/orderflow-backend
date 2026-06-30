using MediatR;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Dlq;
using OrderFlow.Application.Dlq.Commands;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.Orders.Events;
using Serilog.Context;

namespace OrderFlow.Worker.Processors;

public class OrderCreatedDlqMessageProcessor(
    ICorrelationIdAccessor correlationIdAccessor,
    IMessageReceiver messageReceiver,
    IMediator mediator,
    ILogger<OrderCreatedDlqMessageProcessor> logger)
{
    public async Task ProcessAsync(QueueMessage message, CancellationToken stoppingToken)
    {
        try
        {
            correlationIdAccessor.Set(message.CorrelationId);
            using (LogContext.PushProperty("CorrelationId", correlationIdAccessor.CorrelationId))
            {
                logger.LogTrace("DLQ received message: {MessageId} - {Body}", message.MessageId, message.Body);

                var context = new DlqContext( QueueIdentifiers.OrderCreated, nameof(OrderCreatedEvent));
                var command = new ProcessDlqMessageCommand(message, context);
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