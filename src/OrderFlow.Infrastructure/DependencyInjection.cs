using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.EventBus;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure.Alerts;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.Infrastructure.Messaging;
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
        services
            .AddOptions<AwsOptions>()
            .BindConfiguration(AwsOptions.SectionName)
            .Validate(
                AwsOptions.HasValidConfiguration,
                "AWS configuration is invalid.")
            .ValidateOnStart();
        services
            .AddOptions<AlertServiceOptions>()
            .BindConfiguration(AlertServiceOptions.SectionName)
            .Validate(
                AlertServiceOptions.HasValidConfiguration,
                "AlertService configuration is invalid.")
            .ValidateOnStart();
        services
            .AddOptions<SqsOptions>()
            .BindConfiguration(SqsOptions.SectionName);
        services
            .AddOptions<S3FailureStoreOptions>()
            .BindConfiguration(S3FailureStoreOptions.SectionName);
        services
            .AddOptions<OrderCreatedQueueOptions>()
            .BindConfiguration(OrderCreatedQueueOptions.SectionName);
        services
            .AddOptions<OrderCreatedDlqOptions>()
            .BindConfiguration(OrderCreatedDlqOptions.SectionName);
        
        services.AddSingleton<IAmazonSQS>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<AwsOptions>>()
                .Value;

            return new AmazonSQSClient(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                new AmazonSQSConfig
                {
                    ServiceURL = options.ServiceUrl,
                    UseHttp = true
                });
        });
        services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<AwsOptions>>()
                .Value;

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = options.ServiceUrl,
                    ForcePathStyle = true,
                    UseHttp = true
                });
        });

        services.AddScoped<IFailureStore, S3FailureStore>();
        services.AddScoped<IMessageReceiver, SqsMessageReceiver>();
        services.AddScoped<IMessagePublisher, SqsMessagePublisher>();
        services.AddScoped<IQueueIdentifierResolver, QueueIdentifierResolver>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventBus, EventBus.EventBus>();
        
        services.AddHttpClient<IAlertService, AlertService>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<AlertServiceOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.Host);
        });
        services.AddDbContext<OrderFlowDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}