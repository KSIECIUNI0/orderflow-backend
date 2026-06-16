using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public class S3Options
{
    public string BucketName { get; set; } = default!;

    public string Prefix { get; set; } = default!;
}