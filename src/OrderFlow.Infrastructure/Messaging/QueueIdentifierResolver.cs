using Microsoft.Extensions.Options;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.Messaging;

public class QueueIdentifierResolver(
    IOptions<OrderCreatedQueueOptions> orderCreatedQueueOptions,
    IOptions<OrderCreatedDlqOptions> orderCreatedDlqOptions)
    : IQueueIdentifierResolver
{
    private readonly Dictionary<string, string> _queueUrls = new()
    {
        { QueueIdentifiers.OrderCreated, orderCreatedQueueOptions.Value.Url },
        { QueueIdentifiers.OrderCreatedDlq, orderCreatedDlqOptions.Value.Url }
    };

    public string ResolveQueueUrl(string queueIdentifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(queueIdentifier, nameof(queueIdentifier));

        return !_queueUrls.TryGetValue(queueIdentifier, out var url) 
            ? throw new ArgumentOutOfRangeException(nameof(queueIdentifier)) 
            : url;
    }
}