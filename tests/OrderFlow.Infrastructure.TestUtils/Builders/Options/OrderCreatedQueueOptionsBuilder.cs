using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Options;

public class OrderCreatedQueueOptionsBuilder
{
    private string _url = "queueUrl";

    public OrderCreatedQueueOptionsBuilder WithUrl(string value)
    {
        _url = value;
        return this;
    }

    public IOptions<OrderCreatedQueueOptions> Build()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new OrderCreatedQueueOptions
            {
                Url = _url
            });
    }
}