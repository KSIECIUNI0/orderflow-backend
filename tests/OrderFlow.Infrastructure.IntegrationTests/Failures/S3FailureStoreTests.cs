using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Failures;
using OrderFlow.Infrastructure.Configurations;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.Testing.Builders.DTOs;
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
        const string queueName = "test-queue";
        const string messageId = "abc123";
        const string correlationId = "321bca";
        const string eventType = "fakeEventType";
        const string messageBody = "test message body";
        
        var expectedKey = $"{Prefix}/{queueName}/{messageId}.json";
        var options = new S3Options { BucketName = _bucketName, Prefix = Prefix };
        var failedMessage = new FailedMessageBuilder()
            .WithQueueName(queueName)
            .WithMessageId(messageId)
            .WithCorrelationId(correlationId)
            .WithEventType(eventType)
            .WithBody(messageBody)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, Options.Create(options));
        
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
        bucketMessage.QueueName.Should().Be(queueName);
    }
}