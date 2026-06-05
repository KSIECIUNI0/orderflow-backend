using OrderFlow.Application.Correlation;

namespace OrderFlow.Worker.Infrastructure;

public class AsyncCorrelationIdAccessor : ICorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> LocalCorrelationId = new();
    public string? CorrelationId => LocalCorrelationId.Value;
    public void Set(string correlationId)
    {
        LocalCorrelationId.Value = correlationId;
    }
}