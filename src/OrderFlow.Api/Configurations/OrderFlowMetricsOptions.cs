using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Api.Configurations;

[ExcludeFromCodeCoverage]
public sealed class OrderFlowMetricsOptions
{
    public const string SectionName = "OrderFlow:Metrics";

    public double[] OrderCreationDurationBuckets { get; set; } = [];
    
    public static bool HasValidBuckets(OrderFlowMetricsOptions options)
    {
        var buckets = options.OrderCreationDurationBuckets;

        if (buckets is not { Length: > 0 })
            return false;

        for (var i = 0; i < buckets.Length; i++)
        {
            if (!double.IsFinite(buckets[i]) || buckets[i] <= 0)
                return false;

            if (i > 0 && buckets[i] <= buckets[i - 1])
                return false;
        }

        return true;
    }
}