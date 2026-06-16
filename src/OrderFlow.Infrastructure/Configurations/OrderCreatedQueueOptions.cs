using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public class OrderCreatedQueueOptions
{
    public string Url { get; set; } = default!;
}