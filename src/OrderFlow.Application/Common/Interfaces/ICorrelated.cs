namespace OrderFlow.Application.Common.Interfaces;

public interface ICorrelated
{
    string? CorrelationId { get; set; }
}