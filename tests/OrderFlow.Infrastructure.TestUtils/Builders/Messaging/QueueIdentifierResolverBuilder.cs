using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Messaging;

public class QueueIdentifierResolverBuilder
{
    private IOptions<OrderCreatedQueueOptions>
        _orderCreatedQueueOptions = new OrderCreatedQueueOptionsBuilder().Build();
    private IOptions<OrderCreatedDlqOptions> 
        _orderCreatedDlqOptions = new OrderCreatedDlqOptionsBuilder().Build();

    public QueueIdentifierResolverBuilder WithOrderCreatedQueueOptions(IOptions<OrderCreatedQueueOptions> value)
    {
        _orderCreatedQueueOptions = value;
        return this;
    }

    public QueueIdentifierResolverBuilder WithOrderCreatedDlqOptions(IOptions<OrderCreatedDlqOptions> value)
    {
        _orderCreatedDlqOptions = value;
        return this;
    }
    
    public QueueIdentifierResolver Build()
        => new(_orderCreatedQueueOptions, _orderCreatedDlqOptions);
}