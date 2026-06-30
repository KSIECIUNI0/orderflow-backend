using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using Xunit;

namespace OrderFlow.Infrastructure.TestUtils.Fixtures;

public class QueueFixture(SqsContainerFixture containerFixture) : IAsyncLifetime
{
    public SqsDriver Driver { get; private set; } = null!;
    public string QueueUrl { get; private set; } = null!;
    
    public async ValueTask InitializeAsync()
    {
        Driver = new SqsDriver(containerFixture.SqsClient);
        QueueUrl = await Driver.CreateQueueAsync($"test-{Guid.NewGuid()}", TestContext.Current.CancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await Driver.DeleteAsync(QueueUrl, TestContext.Current.CancellationToken);
    }

    public async Task ClearQueueAsync()
    {
        await Driver.ClearAsync(QueueUrl, TestContext.Current.CancellationToken);
    }
}