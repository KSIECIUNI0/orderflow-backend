using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public sealed class S3FailureStoreOptions
{
    public const string SectionName = "S3FailureStore";
    
    public string BucketName { get; set; } = default!;
    public string Prefix { get; set; } = default!;
    public int MaxDegreeOfParallelism { get; set; } = 10;
    public int MaxPageSize { get; set; } = 50;
}