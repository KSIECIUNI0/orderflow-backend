using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Api.IntegrationTests.Helpers;

public class SqsHelper
{
    private readonly IServiceProvider _services;

    public SqsHelper(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<IReadOnlyCollection<Message>?> ReadAllAsync(string queueUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var sqs = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
        
        var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 10,
            VisibilityTimeout = 60,
            MessageAttributeNames = ["*"]
        }, cancellationToken);

        return response?.Messages?.AsReadOnly();
    }

    public async Task ClearAsync(string queueUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var sqs = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();

        var request = new PurgeQueueRequest()
        {
            QueueUrl = queueUrl
        };

        await sqs.PurgeQueueAsync(request, cancellationToken);
    }
}