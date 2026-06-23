using OrderFlow.Api.TestUtils.Factories;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure.TestUtils.Drivers;
using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using Xunit;

namespace OrderFlow.Api.TestUtils.Fixtures;

public class OrdersApiFixture(
    PostgreSqlContainerFixture dbFixture,
    SqsContainerFixture sqsFixture) 
    : IAsyncLifetime
{
    public HttpClient HttpClient { get; private set; } = null!;
    public SqsDriver SqsDriver  { get; private set; } = null!;
    public OrderFlowDbContextDriver DbContextDriver { get; private set; } = null!;
    public string QueueUrl { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var queueName = $"order-created-{Guid.NewGuid()}";
        SqsDriver = new SqsDriver(sqsFixture.SqsClient);
        QueueUrl = await SqsDriver.CreateQueueAsync(queueName, TestContext.Current.CancellationToken);
        
        var dependencyBuilder = new OrderFlowApiDependencyBuilder()
            .WithDb(dbFixture.ConnectionString)
            .WithSqs(sqsFixture.SqsUrl);
        var appConfigBuilder = new OrderFlowApiAppConfigurationBuilder()
            .With("Queues:OrderCreated:Url", QueueUrl);
        var factory = new OrderFlowApiFactory(dependencyBuilder, appConfigBuilder);
        
        HttpClient = factory.CreateClient();
        DbContextDriver = new OrderFlowDbContextDriver(factory.GetDependency<OrderFlowDbContext>());
    }
    
    public async ValueTask DisposeAsync()
    {
        await SqsDriver.DeleteAsync(QueueUrl, TestContext.Current.CancellationToken);
    }

    public async Task Clear()
    {
        await DbContextDriver.ClearAsync(TestContext.Current.CancellationToken);
        await SqsDriver.ClearAsync(QueueUrl, TestContext.Current.CancellationToken);
    }
}