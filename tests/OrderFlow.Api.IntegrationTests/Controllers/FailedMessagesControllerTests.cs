using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.TestUtils.Fixtures;
using OrderFlow.Application.Failures.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.TestUtils.Builders.Failures;
using OrderFlow.Infrastructure.TestUtils.Generators;
using OrderFlow.TestCore.Drivers;

namespace OrderFlow.Api.IntegrationTests.Controllers;

[Collection("Store Containers Tests Collection")]
public class FailedMessagesControllerTests(StoreApiFixture fixture) : IClassFixture<StoreApiFixture>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        await fixture.Clear();
    }
    
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_ShouldReturnEmptyFailedMessageItems_WhenStoreIsEmpty()
    {
        // Arrange
        const int pageSize = 10;
        
        // Act
        var response = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be200Ok()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        
        var content = await response.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
        content.Items.Should().BeEmpty();
        content.IsTruncated.Should().BeFalse();
        content.ContinuationToken.Should().BeNull();
    }
    
    [Theory]
    [InlineData(3)]
    [InlineData(10)] // StoreApiFixture.MaxPageSize => "10"
    public async Task Get_ShouldReturnAllMessages_WhenBucketHasLessOrEqualMessagesThenMaxPageSize(int numberOfMessages)
    {
        // Arrange
        var pageSize = StoreApiFixture.MaxPageSize;
        const string queueId = QueueIdentifiers.OrderCreated;

        var files = S3FileToUploadGenerator.GenerateValidFilesToUpload(numberOfMessages, queueId);
        await fixture.S3Driver.PutObjectsAsync(fixture.BucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var response = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be200Ok()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        
        var content = await response.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
        content.Items.Should().HaveCount(numberOfMessages);
        content.IsTruncated.Should().BeFalse();
        content.ContinuationToken.Should().BeNull();
    }
    
    [Fact]
    public async Task Get_ShouldReturnFirstPage()
    {
        // Arrange
        var pageSize = StoreApiFixture.MaxPageSize;
        const int numberOfMessages = 25;
        const string queueId = QueueIdentifiers.OrderCreated;

        var files = S3FileToUploadGenerator.GenerateValidFilesToUpload(numberOfMessages, queueId);
        await fixture.S3Driver.PutObjectsAsync(fixture.BucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var response = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be200Ok()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        
        var content = await response.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
        content.Items.Should().HaveCount(pageSize);
        content.IsTruncated.Should().BeTrue();
        content.ContinuationToken.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Get_ShouldReturnSecondPage()
    {
        // Arrange
        var pageSize = StoreApiFixture.MaxPageSize;
        var numberOfMessages = StoreApiFixture.MaxPageSize * 2;
        const string queueId = QueueIdentifiers.OrderCreated;

        var files = S3FileToUploadGenerator.GenerateValidFilesToUpload(numberOfMessages, queueId);
        await fixture.S3Driver.PutObjectsAsync(fixture.BucketName, files, TestContext.Current.CancellationToken);
        
        // Act
        var firstPageResponse = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);
        
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        firstPage.Should().NotBeNull();
        firstPage.ContinuationToken.Should().NotBeNull();
        
        var secondPageResponse = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}&continuationToken={firstPage.ContinuationToken}", TestContext.Current.CancellationToken);
        
        // Assert
        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        secondPage.Should().NotBeNull();
        secondPage.Items.Should().HaveCount(pageSize);

        var messageIdsFromFirstPage = firstPage.Items.Select(x => x.MessageId);
        secondPage.Items.Select(x => x.MessageId)
            .Should().NotContain(messageIdsFromFirstPage);
        
        secondPage.IsTruncated.Should().BeFalse();
        secondPage.ContinuationToken.Should().BeNull();
    }
    
    [Fact]
    public async Task Get_ShouldReturnPageItems_WhenOneFileIsInvalid()
    {
        // Arrange
        var pageSize = StoreApiFixture.MaxPageSize;
        const int numberOfValidMessages = 4;
        const int numberOfInvalidMessages = 1;
        const string queueId = QueueIdentifiers.OrderCreated;

        var validFiles = S3FileToUploadGenerator.GenerateValidFilesToUpload(numberOfValidMessages, queueId);
        var invalidFiles = S3FileToUploadGenerator.GenerateInvalidFilesToUpload(numberOfInvalidMessages, queueId);
        await fixture.S3Driver.PutObjectsAsync(fixture.BucketName, [..validFiles, ..invalidFiles], TestContext.Current.CancellationToken);
        
        // Act
        var response = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be200Ok()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        
        var content = await response.Content.ReadFromJsonAsync<FailedMessagePage>(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
        content.Items.Should().HaveCount(numberOfValidMessages);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(10000)]
    public async Task Get_ShouldReturnBadRequest_WhenPageSizeIsInvalid(int pageSize)
    {
        // Act
        var response = await fixture.HttpClient.GetAsync($"/api/failed-messages?pageSize={pageSize}", TestContext.Current.CancellationToken);

        // Assert
        response.Should().Be400BadRequest();
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        
        problem.Should().NotBeNull();
        problem.Status.Should().Be(400);
        problem.Title.Should().Be("Request validation failed");
        problem.Detail.Should().Contain("pageSize");
    }

    [Fact]
    public async Task Resend_ShouldReturn204NoContent_WhenSuccessfullyResendFailedMessage()
    {
        // Arrange
        const string messageId = "abcd1234";
        var correlationId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid();
        const string queueIdentifier = QueueIdentifiers.OrderCreated;

        var @event = new OrderCreatedEvent(orderId);
        var failedMessage = new FailedMessageBuilder()
            .WithMessageId(messageId)
            .WithQueueIdentifier(queueIdentifier)
            .WithCorrelationId(correlationId)
            .WithBody(JsonSerializer.Serialize(@event))
            .Build();
        var key = S3FileToUploadGenerator.BuildKey(messageId, queueIdentifier);
        var file = new S3Driver.FileToUpload(key, JsonSerializer.Serialize(failedMessage));
        await fixture.S3Driver.PutObjectsAsync(fixture.BucketName, [file], TestContext.Current.CancellationToken);
        
        // Act
        var response = await fixture.HttpClient.PostAsync($"/api/failed-messages/{queueIdentifier}/{messageId}/resend", null, TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be204NoContent()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();

        var messages = await fixture.SqsDriver.ReadAllAsync(fixture.QueueUrl, TestContext.Current.CancellationToken);
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
        
        var fileExist = await fixture.S3Driver.FileExistsAsync(fixture.BucketName, key, TestContext.Current.CancellationToken);
        fileExist.Should().BeFalse();
    }

    [Fact]
    public async Task Resend_ShouldReturn400BadRequest_WhenFileDoesNotExists()
    {
        // Arrange
        const string messageId = "message_id_does_not_exist";
        const string queueIdentifier = QueueIdentifiers.OrderCreated;
        
        // Act
        var response = await fixture.HttpClient.PostAsync($"/api/failed-messages/{queueIdentifier}/{messageId}/resend", null, TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be400BadRequest()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        
        problem.Should().NotBeNull();
        problem.Status.Should().Be(400);
        problem.Title.Should().Be("Failed message not found");
        problem.Detail.Should().Be($"Failed message {messageId} was not found");
    }
}