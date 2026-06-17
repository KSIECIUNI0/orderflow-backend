using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Application.Orders.Commands;

public class ProcessOrderCreatedHandler(
    ILogger<ProcessOrderCreatedHandler> logger) 
    : IRequestHandler<ProcessOrderCreatedCommand>
{
    public Task Handle(ProcessOrderCreatedCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ProcessOrderCreatedCommand for OrderId={OrderId} CorrelationId={CorrelationId}",
            request.OrderId,
            request.CorrelationId);

        return Task.CompletedTask;
    }
}