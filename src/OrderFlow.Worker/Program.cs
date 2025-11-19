using Amazon.Runtime;
using Amazon.SQS;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Worker.Orders.Handlers;
using OrderFlow.Worker.Orders.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(OrderCreatedEvent).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OrderCreatedSqsHandler).Assembly);
});

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

builder.Services.AddHostedService<OrderCreatedWorker>();

var host = builder.Build();
await host.RunAsync();
