namespace OrderFlow.Application.Common.Interfaces;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
    void Set(string correlationId);
}