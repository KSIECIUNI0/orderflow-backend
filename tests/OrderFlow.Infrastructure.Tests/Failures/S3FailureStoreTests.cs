using FluentAssertions;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Infrastructure.TestUtils.Builders.Options;
using OrderFlow.Infrastructure.TestUtils.Builders.Stores;

namespace OrderFlow.Infrastructure.Tests.Failures;

public class S3FailureStoreTests
{
    [Fact]
    public async Task SaveAsync_ShouldThrowAnArgumentNullException_WhenFailedMessageIsNull()
    {
        // Arrange
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.SaveAsync(null!, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    public async Task GetPageAsync_ShouldThrowAnArgumentOutOfRangeException_WhenPageSizeIsNegativeOrZero(int pageSize)
    {
        // Arrange
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("pageSize");
    }

    [Fact]
    public async Task GetPageAsync_ShouldThrowAnArgumentOutOfRangeException_WhenPageSizeIsGreaterThenMaxPageSize()
    {
        // Arrange
        const int maxPageSize = 10;
        const int pageSize = maxPageSize + 1;

        var options = new S3FailureStoreOptionsBuilder()
            .WithMaxPageSize(maxPageSize)
            .Build();
        var sut = new S3FailureStoreBuilder()
            .WithS3FailureStoreOptions(options)
            .Build();
        
        // Act
        var act = () => sut.GetPageAsync(pageSize, null, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("pageSize");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetFailedMessageAsync_ShouldThrowAnArgumentException_WhenQueueIdentifierIsNullOrEmpty(
        string? queueIdentifier)
    {
        // Arrange
        const string messageId = "abcd1234";
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.GetFailedMessageAsync(queueIdentifier, messageId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("queueIdentifier");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetFailedMessageAsync_ShouldThrowAnArgumentException_WhenMessageIdIsNullOrEmpty(
        string? messageId)
    {
        // Arrange
        const string queueIdentifier = QueueIdentifiers.OrderCreated;
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.GetFailedMessageAsync(queueIdentifier, messageId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteFailedMessageAsync_ShouldThrowAnArgumentException_WhenQueueIdentifierIsNullOrEmpty(
        string? queueIdentifier)
    {
        // Arrange
        const string messageId = "abcd1234";
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.DeleteFailedMessageAsync(queueIdentifier, messageId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("queueIdentifier");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteFailedMessageAsync_ShouldThrowAnArgumentException_WhenMessageIdIsNullOrEmpty(
        string? messageId)
    {
        // Arrange
        const string queueIdentifier = QueueIdentifiers.OrderCreated;
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.DeleteFailedMessageAsync(queueIdentifier, messageId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }
}