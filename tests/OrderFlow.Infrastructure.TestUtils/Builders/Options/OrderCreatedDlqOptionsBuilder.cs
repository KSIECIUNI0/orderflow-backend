using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Options;

public class OrderCreatedDlqOptionsBuilder
{
    private string _url = "queueUrl";

    public OrderCreatedDlqOptionsBuilder WithUrl(string value)
    {
        _url = value;
        return this;
    }
    
    public IOptions<OrderCreatedDlqOptions> Build()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new OrderCreatedDlqOptions
            {
                Url = _url
            });
    }
}