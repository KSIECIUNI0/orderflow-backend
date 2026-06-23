using Amazon.S3;
using OrderFlow.Api.TestUtils.Factories;
using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using Xunit;

namespace OrderFlow.Api.TestUtils.Fixtures;

public class StoreApiFixture(S3ContainerFixture containerFixture) : IAsyncLifetime
{
    public S3Driver S3Driver { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    public string BucketName { get; private set; } = null!;
    public static string Prefix => "dlq";
    public static int MaxPageSize => 10;
    public static int MaxDegreeOfParallelism => 10;
    
    public async ValueTask InitializeAsync()
    {
        BucketName = $"test-{Guid.NewGuid()}";
        
        var dependencyBuilder = new OrderFlowApiDependencyBuilder()
            .WithS3(containerFixture.S3Url);
        var appConfigBuilder = new OrderFlowApiAppConfigurationBuilder()
            .With("S3FailureStore:BucketName", BucketName)
            .With("S3FailureStore:MaxPageSize", $"{MaxPageSize}")
            .With("S3FailureStore:Prefix", Prefix)
            .With("S3FailureStore:MaxDegreeOfParallelism", $"{MaxDegreeOfParallelism}");

        var apiFactory = new OrderFlowApiFactory(dependencyBuilder, appConfigBuilder);

        HttpClient = apiFactory.CreateClient();
        S3Driver = new S3Driver(apiFactory.GetDependency<IAmazonS3>());
        
        await S3Driver.CreateBucket(BucketName, TestContext.Current.CancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await S3Driver.DeleteBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
    }
    
    public async Task ClearBucket()
    {
        await S3Driver.ClearBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
    }
}