using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Testing.Builders.Handlers;

public class ProcessDlqMessageHandlerBuilder : BaseServiceBuilder<ProcessDlqMessageHandler>
{
    private readonly FakeLogger<ProcessDlqMessageHandler> _logger = new();

    public ProcessDlqMessageHandlerBuilder()
    {
        Inject<ILogger<ProcessDlqMessageHandler>>(_logger);
    }
    
    public FakeLogger<ProcessDlqMessageHandler> GetLogger() => _logger;
    
    public IFailureStore FreezeFailureStore()
    {
        return Freeze<IFailureStore>();
    }

    public IAlertService FreezeAlertService()
    {
        return Freeze<IAlertService>();
    }
}