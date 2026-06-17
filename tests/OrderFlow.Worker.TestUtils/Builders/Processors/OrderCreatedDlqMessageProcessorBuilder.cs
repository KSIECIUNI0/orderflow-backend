using MediatR;
using OrderFlow.Application.Messaging;
using OrderFlow.TestCore.Builders;
using OrderFlow.Worker.Processors;

namespace OrderFlow.Worker.TestUtils.Builders.Processors;

public class OrderCreatedDlqMessageProcessorBuilder : BaseServiceBuilder<OrderCreatedDlqMessageProcessor>
{
    public IMediator FreezeMediator()
    {
        return Freeze<IMediator>();
    }

    public IMessageReceiver FreezeMessageReceiver()
    {
        return Freeze<IMessageReceiver>();
    }
}