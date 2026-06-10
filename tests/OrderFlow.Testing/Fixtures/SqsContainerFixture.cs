using Amazon.Runtime;
using Amazon.SQS;
using Testcontainers.LocalStack;
using Xunit;

namespace OrderFlow.Testing.Fixtures;

public class SqsContainerFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _container = new LocalStackBuilder()
        .WithImage("localstack/localstack:3.0")
        .WithEnvironment("SERVICES", "sqs")
        .WithEnvironment("DEFAULT_REGION", "us-east-1")
        .Build();
    
    public IAmazonSQS SqsClient { get; private set; } = null!;
    public string SqsUrl { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        SqsUrl = _container.GetConnectionString();
        
        SqsClient = new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig
            {
                ServiceURL = SqsUrl,
                UseHttp = true
            });
    }
    
    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}