using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public class OrderCreatedDlqOptions
{
    public string Url { get; set; } = default!;
}