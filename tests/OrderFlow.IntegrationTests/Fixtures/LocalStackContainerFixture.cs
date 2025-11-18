using Amazon.Runtime;
using Amazon.SQS;
using Testcontainers.LocalStack;

namespace OrderFlow.IntegrationTests.Fixtures;

public class LocalStackContainerFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _container;

    public LocalStackContainerFixture()
    {
        _container = new LocalStackBuilder()
            .WithImage("localstack/localstack:3.0")
            .WithEnvironment("SERVICES", "sqs")
            .WithEnvironment("DEFAULT_REGION", "us-east-1")
            .Build();
    }
    
    public string SqsUrl => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var sqsClient = new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig
            {
                ServiceURL = SqsUrl,
                UseHttp = true
            });

        await sqsClient.CreateQueueAsync("OrderCreatedQueue");
    }

    public async Task DisposeAsync()
        => await _container.DisposeAsync();
}