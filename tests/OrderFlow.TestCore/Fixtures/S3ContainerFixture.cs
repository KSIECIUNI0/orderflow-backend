using Amazon.Runtime;
using Amazon.S3;
using Testcontainers.LocalStack;
using Xunit;

namespace OrderFlow.TestCore.Fixtures;

public class S3ContainerFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _container = new LocalStackBuilder()
        .WithImage("localstack/localstack:3.0.2")
        .WithEnvironment("SERVICES", "s3")
        .Build();
    
    public IAmazonS3 S3Client { get; private set; } = null!;
    public string S3Url { get; private set; } = null!;
    
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        S3Url = _container.GetConnectionString();
        
        S3Client = new AmazonS3Client(
            new BasicAWSCredentials("test", "test"),
            new AmazonS3Config
            {
                AuthenticationRegion = "us-east-1",
                ServiceURL = S3Url,
                UseHttp = true,
                ForcePathStyle = true
            });
    }
    
    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}