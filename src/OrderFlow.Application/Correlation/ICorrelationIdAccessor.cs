namespace OrderFlow.Application.Correlation;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
    void Set(string correlationId);
}