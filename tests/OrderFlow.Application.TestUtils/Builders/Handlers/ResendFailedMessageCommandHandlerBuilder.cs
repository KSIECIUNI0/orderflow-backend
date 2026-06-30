using OrderFlow.Application.Failures;
using OrderFlow.Application.Failures.Commands;
using OrderFlow.Application.Messaging;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Application.TestUtils.Builders.Handlers;

public class ResendFailedMessageCommandHandlerBuilder : BaseServiceBuilder<ResendFailedMessageCommandHandler>
{
    public IFailureStore FreezeFailureStore()
    {
        return Freeze<IFailureStore>();
    }

    public IMessagePublisher FreezeMessagePublisher()
    {
        return Freeze<IMessagePublisher>();
    }
}