namespace OrderFlow.Application.Correlation;

public interface ICorrelated
{
    string? CorrelationId { get; set; }
}