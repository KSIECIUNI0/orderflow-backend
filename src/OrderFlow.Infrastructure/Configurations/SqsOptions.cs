using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class SqsOptions
{
    public const string SectionName = "Sqs";
    
    public int MaxNumberOfMessages { get; set; } = default!;
    public int WaitTimeSeconds { get; set; } = default!;
    public int VisibilityTimeout { get; set; } = default!;
}