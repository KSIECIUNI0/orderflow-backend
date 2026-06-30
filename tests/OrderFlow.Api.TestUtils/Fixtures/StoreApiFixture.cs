using Amazon.S3;
using OrderFlow.Api.TestUtils.Factories;
using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using Xunit;

namespace OrderFlow.Api.TestUtils.Fixtures;

public class StoreApiFixture(
    S3ContainerFixture s3ContainerFixture,
    SqsContainerFixture sqsContainerFixture) 
    : IAsyncLifetime
{
    public S3Driver S3Driver { get; private set; } = null!;
    public SqsDriver SqsDriver { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    public string BucketName { get; private set; } = null!;
    public string QueueUrl { get; private set; } = null!;
    public static string Prefix => "dlq";
    public static int MaxPageSize => 10;
    public static int MaxDegreeOfParallelism => 10;
    
    public async ValueTask InitializeAsync()
    {
        BucketName = $"test-{Guid.NewGuid()}";
        SqsDriver = new SqsDriver(sqsContainerFixture.SqsClient);
        QueueUrl = await SqsDriver.CreateQueueAsync($"order-created-{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        
        var dependencyBuilder = new OrderFlowApiDependencyBuilder()
            .WithS3(s3ContainerFixture.S3Url)
            .WithSqs(sqsContainerFixture.SqsUrl);
        var appConfigBuilder = new OrderFlowApiAppConfigurationBuilder()
            .With("S3FailureStore:BucketName", BucketName)
            .With("S3FailureStore:MaxPageSize", $"{MaxPageSize}")
            .With("S3FailureStore:Prefix", Prefix)
            .With("S3FailureStore:MaxDegreeOfParallelism", $"{MaxDegreeOfParallelism}")
            .With("Queues:OrderCreated:Url", QueueUrl);
        var apiFactory = new OrderFlowApiFactory(dependencyBuilder, appConfigBuilder);

        HttpClient = apiFactory.CreateClient();
        S3Driver = new S3Driver(apiFactory.GetDependency<IAmazonS3>());
        
        await S3Driver.CreateBucket(BucketName, TestContext.Current.CancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await S3Driver.DeleteBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
        await SqsDriver.DeleteAsync(QueueUrl, TestContext.Current.CancellationToken);
    }
    
    public async Task Clear()
    {
        await S3Driver.ClearBucket(BucketName, Prefix, TestContext.Current.CancellationToken);
        await SqsDriver.ClearAsync(QueueUrl, TestContext.Current.CancellationToken);
    }
}