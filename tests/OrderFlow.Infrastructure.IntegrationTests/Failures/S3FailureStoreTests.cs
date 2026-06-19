using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OrderFlow.Application.Failures;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.TestUtils.Builders.Failures;
using OrderFlow.Infrastructure.Failures;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;
using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;

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

        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
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
        var sut = new S3FailureStore(_driver.S3Client, options, logger);
        
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
    
    [Fact]
    public async Task GetPageAsync_ShouldReturnEmptyListItems_WhenBucketIsEmpty()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);
        
        // Act
        var result = await sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.IsTruncated.Should().BeFalse();
        result.ContinuationToken.Should().BeNull();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    public async Task GetPageAsync_ShouldReturnAllMessages_WhenBucketHasLessOrEqualMessagesThenMaxPageSize(int numberOfMessages)
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var files = GenerateValidFilesToUpload(numberOfMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var result = await sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(numberOfMessages);
        result.IsTruncated.Should().BeFalse();
        result.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnFirstPage()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const int numberOfMessages = 25;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var files = GenerateValidFilesToUpload(numberOfMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var result = await sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(pageSize);
        result.IsTruncated.Should().BeTrue();
        result.ContinuationToken.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnSecondPage()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const int numberOfMessages = 20;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var files = GenerateValidFilesToUpload(numberOfMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var firstPage = await sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        var secondPage = await sut.GetPageAsync(pageSize, firstPage.ContinuationToken, TestContext.Current.CancellationToken);
        
        // Assert
        secondPage.Should().NotBeNull();
        secondPage.Items.Should().HaveCount(pageSize);

        var messageIdsFromFirstPage = firstPage.Items.Select(x => x.MessageId);
        secondPage.Items.Select(x => x.MessageId)
            .Should().NotContain(messageIdsFromFirstPage);
        
        secondPage.IsTruncated.Should().BeFalse();
        secondPage.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnValidPageItem()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const string messageId = "abc123";
        const string queueId = QueueIdentifiers.OrderCreated;
        const string key = $"{Prefix}/{queueId}/{messageId}.json";
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var failedMessage = new FailedMessageBuilder()
            .WithMessageId(messageId)
            .WithQueueIdentifier(queueId)
            .Build();
        var file = new S3Driver.FileToUpload(key, JsonSerializer.Serialize(failedMessage));
        await _driver.PutObjectsAsync(_bucketName, [file], TestContext.Current.CancellationToken);
        
        // Act
        var result = await sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        
        var pageItem = result.Items[0];
        pageItem.MessageId.Should().Be(messageId);
        pageItem.QueueIdentifier.Should().Be(queueId);
        pageItem.CorrelationId.Should().Be(failedMessage.CorrelationId);
        pageItem.EventType.Should().Be(failedMessage.EventType);
        pageItem.FailedAt.Should().Be(failedMessage.FailedAt);
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnPageItems_WhenOneFileIsInvalid()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const int numberOfValidMessages = 4;
        const int numberOfInvalidMessages = 1;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var validFiles = GenerateValidFilesToUpload(numberOfValidMessages, queueId);
        var invalidFiles = GenerateInvalidFilesToUpload(numberOfInvalidMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, [..validFiles, ..invalidFiles], TestContext.Current.CancellationToken);
        
        // Act
        var act = () => sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        var result = (await act.Should().NotThrowAsync()).Which;
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(numberOfValidMessages);

        var log = logger.LatestRecord;
        log.Level.Should().Be(LogLevel.Warning);
        log.Exception.Should().BeOfType<JsonException>();
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnEmptyPage_WhenAllFilesAreInvalid()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const int numberOfInvalidMessages = 5;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var invalidFiles = GenerateInvalidFilesToUpload(numberOfInvalidMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, invalidFiles, TestContext.Current.CancellationToken);
        
        // Act
        var act = () => sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        var result = (await act.Should().NotThrowAsync()).Which;
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();

        var logs = logger.Collector.GetSnapshot();
        logs.Should().HaveCount(numberOfInvalidMessages);
        logs.Select(log => log.Level).Should().OnlyContain(level => level == LogLevel.Warning);
        logs.Select(log => log.Exception).Should().AllSatisfy(ex => ex.Should().BeOfType<JsonException>());
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnEmptyPage_WhenAllFilesAreEmpty()
    {
        // Arrange
        const int pageSize = 10;
        const int maxPageSize = 10;
        const int numberOfEmptyMessages = 5;
        const string queueId = QueueIdentifiers.OrderCreated;
        var logger = new FakeLogger<S3FailureStore>();
        var options = new S3FailureStoreOptionsBuilder()
            .WithBucketName(_bucketName)
            .WithPrefix(Prefix)
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStore(_driver.S3Client, options, logger);

        var emptyFiles = GenerateEmptyFilesToUpload(numberOfEmptyMessages, queueId);
        await _driver.PutObjectsAsync(_bucketName, emptyFiles, TestContext.Current.CancellationToken);
        
        // Act
        var act = () => sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        var result = (await act.Should().NotThrowAsync()).Which;
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();

        var logs = logger.Collector.GetSnapshot();
        logs
            .Select(log => log.Level == LogLevel.Warning)
            .Should().BeEmpty();
    }

    private static IEnumerable<S3Driver.FileToUpload> GenerateValidFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetValidContentBody);

        string GetValidContentBody(int number) =>
            JsonSerializer.Serialize(new FailedMessageBuilder()
                .WithMessageId($"MessageId-{number}")
                .WithCorrelationId($"CorrelationId-{number}")
                .WithQueueIdentifier(queueId)
                .Build());
    }
    
    private static IEnumerable<S3Driver.FileToUpload> GenerateInvalidFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetInvalidContentBody);

        string GetInvalidContentBody(int number) => $"invalid content body-{number}";
    }
    
    private static IEnumerable<S3Driver.FileToUpload> GenerateEmptyFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetInvalidContentBody);

        string GetInvalidContentBody(int number) => "null";
    }

    private static IEnumerable<S3Driver.FileToUpload> GenerateFilesToUpload(
        int numberOfFiles,
        string queueId,
        Func<int, string> getContentBody) =>
            Enumerable
                .Range(1, numberOfFiles)
                .Select(number => new S3Driver.FileToUpload(
                    $"{Prefix}/{queueId}/message-{Guid.NewGuid()}.json",
                    getContentBody(number)));
}