using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public class SqsOptions
{
    public int MaxNumberOfMessages { get; set; } = default!;
    public int WaitTimeSeconds { get; set; } = default!;
    public int VisibilityTimeout { get; set; } = default!;
}