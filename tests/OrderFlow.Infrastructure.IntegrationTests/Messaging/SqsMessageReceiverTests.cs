using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;
using OrderFlow.Infrastructure.TestUtils.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.Messaging;

[Collection("SQS Container Collection")]
public class SqsMessageReceiverTests(QueueFixture fixture) : IClassFixture<QueueFixture>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await fixture.ClearQueueAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    
    [Theory]
    [InlineData("abcd1234", "abcd1234")]
    [InlineData(null, "UNKNOWN")]
    [InlineData("", "UNKNOWN")]
    public async Task ReceiveAsync_ShouldReadMessagesAndMapToQueueMessage(
        string? correlationId,
        string expectedCorrelationId)
    {
        // Arrange
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var @event = new OrderCreatedEvent(orderId);
        var messageId = await fixture.Driver.SendMessageAsync(@event, fixture.QueueUrl, correlationId, TestContext.Current.CancellationToken);
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(fixture.Driver.SqsClient, options);
        
        // Act
        var result = await sut.ReceiveAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should()
            .NotBeEmpty()
            .And
            .HaveCount(1);
        
        var message = result.Single();
        message.Should().NotBeNull();
        message.CorrelationId.Should().Be(expectedCorrelationId);
        message.Body.Should().Be(JsonSerializer.Serialize(@event));
        message.MessageId.Should().Be(messageId);
        message.ReceiptHandle.Should().NotBeNullOrEmpty();
        message.QueueUrl.Should().Be(fixture.QueueUrl);
    }

    [Fact]
    public async Task ReceiveAsync_ShouldReturnEmptyCollection_WhenNoMessagesReceived()
    {
        // Arrange
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(fixture.Driver.SqsClient, options);
        
        // Act
        var result = await sut.ReceiveAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task CompleteAsync_ShouldCompleteMessageAndDeleteItFromQueue()
    {
        // Arrange
        const string correlationId = "abcd1234";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var @event = new OrderCreatedEvent(orderId);
        await fixture.Driver.SendMessageAsync(@event, fixture.QueueUrl, correlationId, TestContext.Current.CancellationToken);
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(fixture.Driver.SqsClient, options);
        
        // Act
        var receivedMessages = await sut.ReceiveAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
        receivedMessages.Should()
            .NotBeEmpty()
            .And
            .HaveCount(1);
        var receivedMessage = receivedMessages.Single();
        
        await sut.CompleteAsync(receivedMessage, TestContext.Current.CancellationToken);
        
        // Assert
        var receivedMessagesAfterDelete = await sut.ReceiveAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
        receivedMessagesAfterDelete.Should().BeEmpty();
    }
}