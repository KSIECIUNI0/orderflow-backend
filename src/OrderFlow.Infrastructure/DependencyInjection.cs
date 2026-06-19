using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure.Alerts;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Infrastructure.EventBus;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.Infrastructure.MessageReceivers;
using OrderFlow.Infrastructure.Orders;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<S3FailureStoreOptions>(configuration.GetRequiredSection("S3FailureStore"));
        services.Configure<SqsOptions>(configuration.GetRequiredSection("Sqs"));
        services.Configure<OrderCreatedQueueOptions>(configuration.GetRequiredSection("Queues:OrderCreated"));
        services.Configure<OrderCreatedDlqOptions>(configuration.GetRequiredSection("Queues:OrderCreatedDlq"));
        
        services.AddSingleton<IEventBus, SqsEventBus>();
        services.AddSingleton<IAmazonSQS>(_ =>
        {
            var (serviceUrl, accessKey, secretKey) = GetAwsOptions(configuration);

            return new AmazonSQSClient(
                new BasicAWSCredentials(accessKey, secretKey),
                new AmazonSQSConfig
                {
                    ServiceURL = serviceUrl,
                    UseHttp = true
                });
        });
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var (serviceUrl, accessKey, secretKey) = GetAwsOptions(configuration);

            return new AmazonS3Client(
                new BasicAWSCredentials(accessKey, secretKey),
                new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    ForcePathStyle = true,
                    UseHttp = true
                });
        });

        services.AddScoped<IFailureStore, S3FailureStore>();
        services.AddScoped<IMessageReceiver, SqsMessageReceiver>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddHttpClient<IAlertService, AlertService>(client =>
        {
            var host = GetAlertServiceHost(configuration);
            client.BaseAddress = new Uri(host);
        });
        services.AddDbContext<OrderFlowDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    private static (string, string, string) GetAwsOptions(IConfiguration configuration)
    {
        var awsConfig = configuration.GetRequiredSection("AWS");
        
        var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
        var accessKey = awsConfig.GetValue<string>("AccessKey");
        var secretKey = awsConfig.GetValue<string>("SecretKey");
        
        ArgumentException.ThrowIfNullOrEmpty(serviceUrl);
        ArgumentException.ThrowIfNullOrEmpty(accessKey);
        ArgumentException.ThrowIfNullOrEmpty(secretKey);
        
        return (serviceUrl, accessKey, secretKey);
    }

    private static string GetAlertServiceHost(IConfiguration configuration)
    {
        var alertConfig = configuration.GetRequiredSection("AlertService");
        var host = alertConfig.GetValue<string>("Host");
    
        ArgumentException.ThrowIfNullOrEmpty(host);

        return host;
    }
}