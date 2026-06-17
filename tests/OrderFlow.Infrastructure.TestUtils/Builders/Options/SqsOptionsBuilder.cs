using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Options;

public class SqsOptionsBuilder
{
    private const int MaxNumberOfMessages = 10;
    private const int WaitTimeSeconds = 10;
    private const int VisibilityTimeout = 60;

    public IOptions<SqsOptions> Build()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new SqsOptions
            {
                MaxNumberOfMessages = MaxNumberOfMessages,
                WaitTimeSeconds = WaitTimeSeconds,
                VisibilityTimeout = VisibilityTimeout
            });
    }
}