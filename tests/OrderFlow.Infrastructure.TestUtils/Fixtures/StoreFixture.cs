using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using Xunit;

namespace OrderFlow.Infrastructure.TestUtils.Fixtures;

public class StoreFixture(S3ContainerFixture containerFixture) : IAsyncLifetime
{
    public S3Driver Driver { get; private set; } = null!;
    public string BucketName { get; private set; } = null!;
    public static string Prefix => "dlq";

    public async ValueTask InitializeAsync()
    {
        BucketName = $"test-{Guid.NewGuid()}";
        Driver = new S3Driver(containerFixture.S3Client);
        await Driver.CreateBucket(BucketName, TestContext.Current.CancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await Driver.DeleteBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
    }

    public async Task ClearBucket()
    {
        await Driver.ClearBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
    }
}