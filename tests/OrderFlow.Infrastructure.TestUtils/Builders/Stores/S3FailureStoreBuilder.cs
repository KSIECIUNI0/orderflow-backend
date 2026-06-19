using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Infrastructure.TestUtils.Builders.Stores;

public class S3FailureStoreBuilder : BaseServiceBuilder<S3FailureStore>
{
    private readonly FakeLogger<S3FailureStore> _logger = new();

    public S3FailureStoreBuilder()
    {
        Inject<ILogger<S3FailureStore>>(_logger);
    }

    public S3FailureStoreBuilder WithS3FailureStoreOptions(IOptions<S3FailureStoreOptions> options)
    {
        Inject(options);
        return this;
    }
}