using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Messaging;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.Testing.Builders.DTOs;
using OrderFlow.Testing.Builders.Options;
using OrderFlow.Testing.Drivers;
using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.Failures;

[Collection("S3 Container Collection")]
public class S3FailureStoreTests(S3ContainerFixture fixture) : IAsyncLifetime
{
    private readonly S3Driver _driver = new(fixture.S3Client);
    private string _bucketName = null!;
    private const string Prefix = "dlq";
    
    public async ValueTask InitializeAsync()
    {
        _bucketName = $"test-{Guid.NewGuid()}";
        await _driver.CreateBucket(_bucketName, TestContext.Current.CancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _driver.DeleteBucket(_bucketName, Prefix, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SaveFailedMessageAsync_ShouldPutFailedMessageInTheBucket()
    {
        // Arrange
        const string queueId = QueueIdentifiers.OrderCreated;
        const string messageId = "abc123";
        const string correlationId = "321bca";
        const string eventType = "fakeEventType";
        const string messageBody = "test message body";
        const string expectedKey = $"{Prefix}/{queueId}/{messageId}.json";
        
        var options = new S3OptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .Build();
        var failedMessage = new FailedMessageBuilder()
            .WithQueueIdentifier(queueId)
            .WithMessageId(messageId)
            .WithCorrelationId(correlationId)
            .WithEventType(eventType)
            .WithBody(messageBody)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options);
        
        // Act
        await sut.SaveFailedMessageAsync(failedMessage, TestContext.Current.CancellationToken);
        
        // Assert
        var fileExists = await _driver.FileExistsAsync(_bucketName, expectedKey, TestContext.Current.CancellationToken);
        fileExists.Should().BeTrue();
        
        var content = await _driver.GetFileContentAsTextAsync(_bucketName, expectedKey, TestContext.Current.CancellationToken);
        content.Should().NotBeNullOrEmpty();
        
        var bucketMessage = JsonSerializer.Deserialize<FailedMessage>(content);
        bucketMessage.Should().NotBeNull();
        bucketMessage.MessageId.Should().Be(messageId);
        bucketMessage.CorrelationId.Should().Be(correlationId);
        bucketMessage.EventType.Should().Be(eventType);
        bucketMessage.Body.Should().Be(messageBody);
        bucketMessage.QueueIdentifier.Should().Be(queueId);
    }
}