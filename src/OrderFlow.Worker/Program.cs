using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using MediatR;
using OrderFlow.Application.Common.Behaviors;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Worker.Infrastructure;
using OrderFlow.Worker.Orders.Handlers;
using OrderFlow.Worker.Orders.Workers;
using OrderFlow.Worker.Shared.AlertService;
using OrderFlow.Worker.Shared.FailureStore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(OrderCreatedEvent).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OrderCreatedSqsHandler).Assembly);
});

builder.Services.AddSingleton<ICorrelationIdAccessor, AsyncCorrelationIdAccessor>();
builder.Services.AddSingleton<IFailureStore, S3FailureStore>();
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
    var accessKey = awsConfig.GetValue<string>("AccessKey");
    var secretKey = awsConfig.GetValue<string>("SecretKey");

    return new AmazonSQSClient(
        new BasicAWSCredentials(accessKey, secretKey),
        new AmazonSQSConfig
        {
            ServiceURL = serviceUrl,
            UseHttp = true
        });
});
builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
    var accessKey = awsConfig.GetValue<string>("AccessKey");
    var secretKey = awsConfig.GetValue<string>("SecretKey");

    return new AmazonS3Client(
        new BasicAWSCredentials(accessKey, secretKey),
        new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            UseHttp = true
        });
});
builder.Services.AddHttpClient<IAlertService, AlertService>(client =>
{
    var alertConfig = builder.Configuration.GetSection("AlertService");
    var host = alertConfig.GetValue<string>("Host");
    
    ArgumentException.ThrowIfNullOrEmpty(host);
    
    client.BaseAddress = new Uri(host);
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationIdBehavior<,>));

builder.Services.AddHostedService<OrderCreatedWorker>();
builder.Services.AddHostedService<OrderCreatedDlqWorker>();

var host = builder.Build();
await host.RunAsync();
