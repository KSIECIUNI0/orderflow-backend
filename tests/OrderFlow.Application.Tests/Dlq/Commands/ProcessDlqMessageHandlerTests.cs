using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Alerts.DTOs;
using OrderFlow.Application.Failures.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.TestUtils.Builders.Commands;
using OrderFlow.Application.TestUtils.Builders.Dlq;
using OrderFlow.Application.TestUtils.Builders.DTOs;
using OrderFlow.Application.TestUtils.Builders.Handlers;
using OrderFlow.TestCore.Fakes;

namespace OrderFlow.Application.Tests.Dlq.Commands;

public class ProcessDlqMessageHandlerTests
{
    [Fact]
    public async Task Handle_ShouldProcessDlqMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        const string eventType = "FakeEventType";
        const string body = "FakeBody";
        var queueMessage = new QueueMessageBuilder()
            .WithMessageId(messageId)
            .WithCorrelationId(correlationId)
            .WithBody(body)
            .Build();
        var dlqContext = new DlqContextBuilder()
            .WithEventType(eventType)
            .WithQueueIdentifier(QueueIdentifiers.OrderCreated)
            .Build();
        var command = new ProcessDlqMessageCommandBuilder()
            .WithQueueMessage(queueMessage)
            .WithDlqContext(dlqContext)
            .Build();
        var builder = new ProcessDlqMessageHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        var alertService = builder.FreezeAlertService();
        var logger = builder.GetLogger();
        var sut = builder.Build();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var log = logger.Collector.GetSnapshot()
            .FirstOrDefault(l => l.Level == LogLevel.Error);

        log.Should().NotBeNull();
        log.State.Should().NotBeNull();
        var logState = log.State
            .Should()
            .BeAssignableTo<IEnumerable<KeyValuePair<string, string>>>()
            .Subject;
        logState
            .Should()
            .ContainKeys("CorrelationId", "MessageId", "Body")
            .And
            .ContainValues(correlationId, messageId, body);

        await failureStore
            .Received(1)
            .SaveAsync(
                Arg.Is<FailedMessage>(message =>
                    message.MessageId == messageId &&
                    message.QueueIdentifier == QueueIdentifiers.OrderCreated &&
                    message.EventType == eventType &&
                    message.CorrelationId == correlationId &&
                    message.Body == body),
                Arg.Any<CancellationToken>());

        await alertService
            .Received(1)
            .SendAsync(
        Arg.Is<AlertRequest>(alert =>
                    alert.MessageId == messageId &&
                    alert.CorrelationId == correlationId &&
                    alert.Type == "ERROR" &&
                    alert.Error == "The message reached DLQ"),
        Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenFailureStoreFails()
    {
        // Arrange
        const string exceptionMessage = "Fake Exception";
        var fakeException = new FakeException(exceptionMessage);
        var command = new ProcessDlqMessageCommandBuilder().Build();
        var builder = new ProcessDlqMessageHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        var alertService = builder.FreezeAlertService();

        failureStore
            .SaveAsync(Arg.Any<FailedMessage>(), Arg.Any<CancellationToken>())
            .Throws(fakeException);
        
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(exceptionMessage);
        
        await alertService
            .DidNotReceive()
            .SendAsync(Arg.Any<AlertRequest>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldPropagateException_WhenAlertServiceFails()
    {
        // Arrange
        const string exceptionMessage = "Fake Exception";
        var fakeException = new FakeException(exceptionMessage);
        var command = new ProcessDlqMessageCommandBuilder().Build();
        var builder = new ProcessDlqMessageHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        var alertService = builder.FreezeAlertService();

        alertService
            .SendAsync(Arg.Any<AlertRequest>(), Arg.Any<CancellationToken>())
            .Throws(fakeException);
        
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(exceptionMessage);

        await failureStore
            .Received(1)
            .SaveAsync(Arg.Any<FailedMessage>(), Arg.Any<CancellationToken>());
    }
}