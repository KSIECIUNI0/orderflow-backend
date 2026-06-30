using FluentAssertions;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.TestUtils.Builders.DTOs;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.TestUtils.Builders.Messaging;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;
using OrderFlow.Infrastructure.TestUtils.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.Messaging;

[Collection("SQS Container Collection")]
public class SqsMessagePublisherTests(QueueFixture fixture) : IClassFixture<QueueFixture>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await fixture.ClearQueueAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    
    [Fact]
    public async Task PublishAsync_ShouldSendMessageToQueueWithCorrelationId()
    {
        // Arrange
        const string correlationId = "xyz123";
        const string messageBody = "message_body";
        var outgoingMessage = new OutgoingMessageBuilder()
            .WithQueueIdentifier(QueueIdentifiers.OrderCreated)
            .WithCorrelationId(correlationId)
            .WithBody(messageBody)
            .Build();
        
        var orderCreatedQueueOptions = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(fixture.QueueUrl)
            .Build();
        var queueIdentifierResolver = new QueueIdentifierResolverBuilder()
            .WithOrderCreatedQueueOptions(orderCreatedQueueOptions)
            .Build();
        
        var sut = new SqsMessagePublisher(fixture.Driver.SqsClient, queueIdentifierResolver);
        
        // Act
        await sut.PublishAsync(outgoingMessage, TestContext.Current.CancellationToken);
        
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
        
        message.Body.Should().NotBeNull();
        message.Body.Should().Be(messageBody);
    }
    
    [Fact]
    public async Task PublishAsync_ShouldSendMessageToQueueWithoutCorrelationId()
    {
        // Arrange
        const string messageBody = "message_body";
        var outgoingMessage = new OutgoingMessageBuilder()
            .WithQueueIdentifier(QueueIdentifiers.OrderCreated)
            .WithoutCorrelationId()
            .WithBody(messageBody)
            .Build();
        
        var orderCreatedQueueOptions = new OrderCreatedQueueOptionsBuilder()
            .WithUrl(fixture.QueueUrl)
            .Build();
        var queueIdentifierResolver = new QueueIdentifierResolverBuilder()
            .WithOrderCreatedQueueOptions(orderCreatedQueueOptions)
            .Build();
        
        var sut = new SqsMessagePublisher(fixture.Driver.SqsClient, queueIdentifierResolver);
        
        // Act
        await sut.PublishAsync(outgoingMessage, TestContext.Current.CancellationToken);
        
        // Assert
        var messages = await fixture.Driver.ReadAllAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
        messages.Should().NotBeNullOrEmpty();
        messages.Should().HaveCount(1);
        var message = messages.First();
        
        var messageCorrelationId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
            ? attr.StringValue
            : null;
        messageCorrelationId.Should().BeNull();
        
        message.Body.Should().NotBeNull();
        message.Body.Should().Be(messageBody);
    }
}