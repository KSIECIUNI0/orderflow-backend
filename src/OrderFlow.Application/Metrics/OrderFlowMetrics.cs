using System.Diagnostics.Metrics;

namespace OrderFlow.Application.Metrics;

public sealed class OrderFlowMetrics : IDisposable
{
    public const string MeterName = "OrderFlow.Application";

    private readonly Meter _meter;
    private readonly Counter<long> _orderCreationOperations;
    private static readonly KeyValuePair<string, object?> SuccessTag = new("result", "success");
    private static readonly KeyValuePair<string, object?> FailureTag = new("result", "failure");
    private readonly Histogram<double> _orderCreationDuration;

    public OrderFlowMetrics()
    {
        _meter = new Meter(MeterName);

        _orderCreationOperations = _meter.CreateCounter<long>(
            name: "orderflow.orders.creation.operations",
            unit: "{operation}",
            description: "Number of order creation handler executions by result.");
        
        _orderCreationDuration = _meter.CreateHistogram<double>(
            name: "orderflow.orders.creation.duration",
            unit: "s",
            description: "Duration of order creation handler executions.");
    }
    
    public void RecordOrderCreationSucceeded(double durationInSeconds)
    {
        _orderCreationOperations.Add(1, SuccessTag);
        _orderCreationDuration.Record(durationInSeconds, SuccessTag);
    }

    public void RecordOrderCreationFailed(double durationInSeconds)
    {
        _orderCreationOperations.Add(1, FailureTag);
        _orderCreationDuration.Record(durationInSeconds, FailureTag);
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}