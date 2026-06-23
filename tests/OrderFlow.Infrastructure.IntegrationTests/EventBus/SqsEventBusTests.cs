using FluentAssertions;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.EventBus;
using System.Text.Json;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;
using OrderFlow.Infrastructure.TestUtils.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.EventBus;

[Collection("SQS Container Collection")]
public class SqsEventBusTests(QueueFixture fixture) : IClassFixture<QueueFixture>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await fixture.ClearQueueAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    
    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldSendEventToQueueWithCorrelationId()
    {
        // Arrange
        const string correlationId = "xyz123";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var options = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(fixture.QueueUrl)
            .Build();
        var @event = new OrderCreatedEvent(orderId);
        
        var sut = new SqsEventBus(fixture.Driver.SqsClient, options);
        
        // Act
        await sut.PublishOrderCreatedAsync(@event, correlationId, TestContext.Current.CancellationToken);
        
        // Assert
        var messages = await fixture.Driver.ReadAllAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
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
            .WithUrl(fixture.QueueUrl)
            .Build();
        var @event = new OrderCreatedEvent(orderId);
        
        var sut = new SqsEventBus(fixture.Driver.SqsClient, options);
        
        // Act
        await sut.PublishOrderCreatedAsync(@event, null, TestContext.Current.CancellationToken);
        
        // Assert
        var messages = await fixture.Driver.ReadAllAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
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