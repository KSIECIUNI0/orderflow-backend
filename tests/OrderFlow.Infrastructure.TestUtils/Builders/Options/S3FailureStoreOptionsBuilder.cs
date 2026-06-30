using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Options;

public class S3FailureStoreOptionsBuilder
{
    private string _bucketName = "testBucket";
    private string _prefix = "tests";
    private int _maxPageSize = 50;
    private const int MaxDegreeOfParallelism = 10;

    public S3FailureStoreOptionsBuilder WithBucketName(string value)
    {
        _bucketName = value;
        return this;
    }

    public S3FailureStoreOptionsBuilder WithPrefix(string value)
    {
        _prefix = value;
        return this;
    }

    public S3FailureStoreOptionsBuilder WithMaxPageSize(int value)
    {
        _maxPageSize = value;
        return this;
    }

    public IOptions<S3FailureStoreOptions> Build()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new S3FailureStoreOptions
            {
                BucketName = _bucketName,
                Prefix = _prefix,
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                MaxPageSize = _maxPageSize
            });
    }
}