using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Infrastructure;

namespace OrderFlow.Api.TestUtils.Factories;

public class OrderFlowApiDependencyBuilder
{
    private string? _connectionString;
    private string? _sqsUrl;
    private string? _s3Url;

    public OrderFlowApiDependencyBuilder WithDb(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public OrderFlowApiDependencyBuilder WithSqs(string sqsUrl)
    {
        _sqsUrl = sqsUrl;
        return this;
    }

    public OrderFlowApiDependencyBuilder WithS3(string s3Url)
    {
        _s3Url = s3Url;
        return this;
    }

    public void Build(IServiceCollection services)
    {
        if (!string.IsNullOrEmpty(_connectionString))
        {
            ConfigureDbService(services);
        }

        if (!string.IsNullOrEmpty(_sqsUrl))
        {
            ConfigureSqsService(services);
        }

        if (!string.IsNullOrEmpty(_s3Url))
        {
            ConfigureS3Service(services);
        }
    }

    private void ConfigureDbService(IServiceCollection services)
    {
        services.RemoveAll(typeof(DbContextOptions<OrderFlowDbContext>));
        services.AddDbContext<OrderFlowDbContext>(options =>
        {
            options.UseNpgsql(_connectionString);
        });
        
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
        db.Database.Migrate();
    }

    private void ConfigureSqsService(IServiceCollection services)
    {
        services.RemoveAll(typeof(IAmazonSQS));
        services.AddSingleton<IAmazonSQS>(_ =>
        {
            var config = new AmazonSQSConfig
            {
                ServiceURL = _sqsUrl,
                UseHttp = true
            };
            return new AmazonSQSClient(
                new BasicAWSCredentials("test", "test"),
                config);
        });
    }

    private void ConfigureS3Service(IServiceCollection services)
    {
        services.RemoveAll(typeof(IAmazonS3));
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _s3Url,
                ForcePathStyle = true,
                UseHttp = true
            };
            return new AmazonS3Client(
                new BasicAWSCredentials("test", "test"),
                config);
        });
    }
}