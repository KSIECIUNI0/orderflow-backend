using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.Alerts.DTOs;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Failures.DTOs;
using OrderFlow.Application.Messaging.DTOs;

namespace OrderFlow.Application.Dlq.Commands;

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

        await SaveFailedMessageAsync(request, cancellationToken);
        await SendAlertAsync(request.QueueMessage, cancellationToken);
    }
    
    private async Task SaveFailedMessageAsync(
        ProcessDlqMessageCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving failed message to FailureStore...");
        var failedMessage = new FailedMessage
        {
            MessageId = command.QueueMessage.MessageId,
            QueueIdentifier = command.Context.QueueIdentifier,
            EventType = command.Context.EventType,
            CorrelationId = command.QueueMessage.CorrelationId,
            Body = command.QueueMessage.Body
        };
        
        await failureStore.SaveAsync(failedMessage, cancellationToken);
    }

    private async Task SendAlertAsync(
        QueueMessage message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending alert request to AlertService...");
        var request = new AlertRequest
        {
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            Type = "ERROR",
            Error = "The message reached DLQ"
        };
            
        await alertService.SendAsync(request, cancellationToken);
    }
}