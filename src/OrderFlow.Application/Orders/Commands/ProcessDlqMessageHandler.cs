using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Messaging;

namespace OrderFlow.Application.Orders.Commands;

public class ProcessDlqMessageHandler(
    IFailureStore failureStore,
    IAlertService alertService,
    ILogger<ProcessDlqMessageHandler> logger) : IRequestHandler<ProcessDlqMessageCommand>
{
    public async Task Handle(ProcessDlqMessageCommand request, CancellationToken cancellationToken)
    {
        logger.LogError(
            "DLQ message received. CorrelationId={CorrelationId}, MessageId={MessageId}, Body={Body}",
            request.QueueMessage.CorrelationId,
            request.QueueMessage.MessageId,
            request.QueueMessage.Body);

        await SaveFailedMessageAsync(request.QueueMessage, request.EventType, request.QueueName);
        await SendAlertAsync(request.QueueMessage);
    }
    
    private async Task SaveFailedMessageAsync(
        QueueMessage message,
        string eventType,
        string queueName)
    {
        logger.LogInformation("Saving failed message to FailureStore...");
        var failedMessage = new FailedMessage
        {
            MessageId = message.MessageId,
            QueueName = queueName,
            EventType = eventType,
            CorrelationId = message.CorrelationId,
            Body = message.Body
        };
        
        await failureStore.SaveFailedMessageAsync(failedMessage);
    }

    private async Task SendAlertAsync(QueueMessage message)
    {
        logger.LogInformation("Sending alert request to AlertService...");
        var request = new AlertRequest
        {
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            Type = "ERROR",
            Error = "The message reached DLQ"
        };
            
        await alertService.SendAsync(request);
    }
}