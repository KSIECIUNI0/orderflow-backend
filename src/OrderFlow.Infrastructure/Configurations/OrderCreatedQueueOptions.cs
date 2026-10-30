using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedQueueOptions
{
    public const string SectionName = "Queues:OrderCreated";
    
    public string Url { get; set; } = default!;
}