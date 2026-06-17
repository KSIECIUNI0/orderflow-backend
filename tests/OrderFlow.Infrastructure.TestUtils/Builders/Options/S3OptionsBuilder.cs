using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Options;

public class S3OptionsBuilder
{
    private string _bucketName = "testBucket";
    private string _prefix = "tests";

    public S3OptionsBuilder WithBucketName(string value)
    {
        _bucketName = value;
        return this;
    }

    public S3OptionsBuilder WithPrefix(string value)
    {
        _prefix = value;
        return this;
    }

    public IOptions<S3Options> Build()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new S3Options
            {
                BucketName = _bucketName,
                Prefix = _prefix
            });
    }
}