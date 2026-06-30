using OrderFlow.Application.Messaging;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Infrastructure.TestUtils.Builders.EventBus;

public class EventBusBuilder : BaseServiceBuilder<Infrastructure.EventBus.EventBus>
{
    public IMessagePublisher FreezeMessagePublisher()
    {
        return Freeze<IMessagePublisher>();
    }
}