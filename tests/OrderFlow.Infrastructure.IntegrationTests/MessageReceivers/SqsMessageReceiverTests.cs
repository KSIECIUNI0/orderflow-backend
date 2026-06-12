using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.MessageReceivers;
using OrderFlow.Testing.Builders.Options;
using OrderFlow.Testing.Drivers;
using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.MessageReceivers;

[Collection("SQS Container Collection")]
public class SqsMessageReceiverTests(SqsContainerFixture fixture) : IAsyncLifetime
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
        var messageId = await _driver.SendMessageAsync(@event, _queueUrl, correlationId, TestContext.Current.CancellationToken);
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(_driver.SqsClient, options);
        
        // Act
        var result = await sut.ReceiveAsync(_queueUrl, TestContext.Current.CancellationToken);
        
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
    }

    [Fact]
    public async Task ReceiveAsync_ShouldReturnEmptyCollection_WhenNoMessagesReceived()
    {
        // Arrange
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(_driver.SqsClient, options);
        
        // Act
        var result = await sut.ReceiveAsync(_queueUrl, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteMessage()
    {
        // Arrange
        const string correlationId = "abcd1234";
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var @event = new OrderCreatedEvent(orderId);
        await _driver.SendMessageAsync(@event, _queueUrl, correlationId, TestContext.Current.CancellationToken);
        var options = new SqsOptionsBuilder().Build();
        var sut = new SqsMessageReceiver(_driver.SqsClient, options);
        
        // Act
        var receivedMessages = await sut.ReceiveAsync(_queueUrl, TestContext.Current.CancellationToken);
        receivedMessages.Should()
            .NotBeEmpty()
            .And
            .HaveCount(1);
        var receivedMessage = receivedMessages.Single();
        
        await sut.DeleteAsync(_queueUrl, receivedMessage.ReceiptHandle, TestContext.Current.CancellationToken);
        
        // Assert
        var receivedMessagesAfterDelete = await sut.ReceiveAsync(_queueUrl, TestContext.Current.CancellationToken);
        receivedMessagesAfterDelete.Should().BeEmpty();
    }
}