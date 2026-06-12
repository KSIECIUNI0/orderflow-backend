using FluentAssertions;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.EventBus;
using System.Text.Json;
using OrderFlow.Testing.Builders.Options;
using OrderFlow.Testing.Drivers;
using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.EventBus;

[Collection("SQS Container Collection")]
public class SqsEventBusTests(SqsContainerFixture fixture) : IAsyncLifetime
{
    private readonly SqsDriver _driver = new(fixture.SqsClient);
    private string _queueUrl = null!;

    public async ValueTask InitializeAsync()
    {
        _queueUrl = await _driver.CreateQueueAsync($"test-{Guid.NewGuid()}", TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _driver.ClearAsync(_queueUrl, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldSendEventToQueueWithCorrelationId()
    {
        // Arrange
        const string correlationId = "xyz123";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var options = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(_queueUrl)
            .Build();
        var @event = new OrderCreatedEvent(orderId);
        
        var sut = new SqsEventBus(_driver.SqsClient, options);
        
        // Act
        await sut.PublishOrderCreatedAsync(@event, correlationId, TestContext.Current.CancellationToken);
        
        // Assert
        var messages = await _driver.ReadAllAsync(_queueUrl, TestContext.Current.CancellationToken);
        messages.Should().NotBeNullOrEmpty();
        messages.Should().HaveCount(1);
        var message = messages.First();
        
        var messageCorrelationId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
            ? attr.StringValue
            : null;
        messageCorrelationId.Should().NotBeNullOrEmpty();
        messageCorrelationId.Should().Be(correlationId);
        
        var messageBody = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
        messageBody.Should().NotBeNull();
        messageBody.OrderId.Should().Be(orderId);
    }
    
    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldSendEventToQueueWithoutCorrelationId()
    {
        // Arrange
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var options = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(_queueUrl)
            .Build();
        var @event = new OrderCreatedEvent(orderId);
        
        var sut = new SqsEventBus(_driver.SqsClient, options);
        
        // Act
        await sut.PublishOrderCreatedAsync(@event, null, TestContext.Current.CancellationToken);
        
        // Assert
        var messages = await _driver.ReadAllAsync(_queueUrl, TestContext.Current.CancellationToken);
        messages.Should().NotBeNullOrEmpty();
        messages.Should().HaveCount(1);
        var message = messages.First();
        
        var messageCorrelationId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
            ? attr.StringValue
            : null;
        messageCorrelationId.Should().BeNull();
        
        var messageBody = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
        messageBody.Should().NotBeNull();
        messageBody.OrderId.Should().Be(orderId);
    }
}