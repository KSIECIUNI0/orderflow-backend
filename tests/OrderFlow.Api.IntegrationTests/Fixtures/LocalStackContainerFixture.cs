using Amazon.Runtime;
using Amazon.SQS;
using Testcontainers.LocalStack;

namespace OrderFlow.Api.IntegrationTests.Fixtures;

public class LocalStackContainerFixture : IAsyncLifetime
{
    private const string QueueName = "order-created-queue";
    private readonly LocalStackContainer _container;
    private string? _queueUrl;

    public LocalStackContainerFixture()
    {
        _container = new LocalStackBuilder()
            .WithImage("localstack/localstack:3.0")
            .WithEnvironment("SERVICES", "sqs")
            .WithEnvironment("DEFAULT_REGION", "us-east-1")
            .Build();
    }
    
    public string SqsUrl => _container.GetConnectionString();
    
    public string QueueUrl => _queueUrl ?? QueueName;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        
        var sqsClient = new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig
            {
                ServiceURL = SqsUrl,
                UseHttp = true
            });

        var createQueueResponse = await sqsClient.CreateQueueAsync(QueueName);
        _queueUrl = createQueueResponse.QueueUrl;
    }

    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}