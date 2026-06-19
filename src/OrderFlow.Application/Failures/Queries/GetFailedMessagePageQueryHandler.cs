using MediatR;
using OrderFlow.Application.Failures.DTOs;

namespace OrderFlow.Application.Failures.Queries;

public class GetFailedMessagePageQueryHandler(IFailureStore failureStore) : IRequestHandler<GetFailedMessagePageQuery, FailedMessagePage>
{
    public async Task<FailedMessagePage> Handle(
        GetFailedMessagePageQuery request,
        CancellationToken cancellationToken)
    {
        return await failureStore.GetPageAsync(request.PageSize, request.ContinuationToken, cancellationToken);
    }
}