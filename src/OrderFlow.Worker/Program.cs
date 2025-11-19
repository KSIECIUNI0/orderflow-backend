using Amazon.Runtime;
using Amazon.SQS;
using OrderFlow.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
    var region =  awsConfig.GetValue<string>("Region");
    var accessKey = awsConfig.GetValue<string>("AccessKey");
    var secretKey = awsConfig.GetValue<string>("SecretKey");

    return new AmazonSQSClient(
        new BasicAWSCredentials(accessKey, secretKey),
        new AmazonSQSConfig
        {
            ServiceURL = serviceUrl,
            AuthenticationRegion = region,
            UseHttp = true
        });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
