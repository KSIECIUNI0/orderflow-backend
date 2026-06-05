namespace OrderFlow.Infrastructure.Configurations;

public class S3Options
{
    public string BucketName { get; set; } = default!;

    public string Prefix { get; set; } = default!;
}