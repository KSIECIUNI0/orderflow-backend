using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedDlqOptions
{
    public const string SectionName = "Queues:OrderCreatedDlq";
    
    public string Url { get; set; } = default!;
}