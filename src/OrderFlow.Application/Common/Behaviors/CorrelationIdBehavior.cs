using MediatR;
using OrderFlow.Application.Common.Interfaces;

namespace OrderFlow.Application.Common.Behaviors;

public class CorrelationIdBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public CorrelationIdBehavior(ICorrelationIdAccessor correlationIdAccessor)
    {
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ICorrelated correlated)
        {
            correlated.CorrelationId ??= _correlationIdAccessor.CorrelationId;
        }
        
        return await next(cancellationToken);
    }
}