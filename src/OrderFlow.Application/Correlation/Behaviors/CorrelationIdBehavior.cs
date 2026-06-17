using MediatR;

namespace OrderFlow.Application.Correlation.Behaviors;

public class CorrelationIdBehavior<TRequest, TResponse>(ICorrelationIdAccessor correlationIdAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ICorrelated correlated)
        {
            correlated.CorrelationId ??= correlationIdAccessor.CorrelationId;
        }
        
        return await next(cancellationToken);
    }
}