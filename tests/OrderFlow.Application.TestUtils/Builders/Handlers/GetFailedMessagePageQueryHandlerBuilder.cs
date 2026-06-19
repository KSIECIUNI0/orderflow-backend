using OrderFlow.Application.Failures;
using OrderFlow.Application.Failures.Queries;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Application.TestUtils.Builders.Handlers;

public class GetFailedMessagePageQueryHandlerBuilder : BaseServiceBuilder<GetFailedMessagePageQueryHandler>
{
    public IFailureStore FreezeFailureStore()
    {
        return Freeze<IFailureStore>();
    }
}