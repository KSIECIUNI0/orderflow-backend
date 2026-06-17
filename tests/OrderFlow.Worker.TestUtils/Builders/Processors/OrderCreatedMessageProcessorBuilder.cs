using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Messaging;
using OrderFlow.TestCore.Builders;
using OrderFlow.Worker.Processors;

namespace OrderFlow.Worker.TestUtils.Builders.Processors;

public class OrderCreatedMessageProcessorBuilder : BaseServiceBuilder<OrderCreatedMessageProcessor>
{
    private readonly FakeLogger<OrderCreatedMessageProcessor> _logger = new();

    public OrderCreatedMessageProcessorBuilder()
    {
        Inject<ILogger<OrderCreatedMessageProcessor>>(_logger);
    }
    
    public FakeLogger<OrderCreatedMessageProcessor> GetLogger() => _logger;
    
    public IMediator FreezeMediator()
    {
        return Freeze<IMediator>();
    }

    public IMessageReceiver FreezeMessageReceiver()
    {
        return Freeze<IMessageReceiver>();
    }
    
    public ICorrelationIdAccessor FreezeCorrelationIdAccessor()
    {
        return Freeze<ICorrelationIdAccessor>();
    }
}