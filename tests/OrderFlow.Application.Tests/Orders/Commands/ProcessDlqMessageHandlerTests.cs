using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Alerts;
using OrderFlow.Application.Failures;
using OrderFlow.Testing.Builders.Commands;
using OrderFlow.Testing.Builders.DTOs;
using OrderFlow.Testing.Builders.Handlers;
using OrderFlow.Testing.Fakes;

namespace OrderFlow.Application.Tests.Orders.Commands;

public class ProcessDlqMessageHandlerTests
{
    [Fact]
    public async Task Handle_ShouldProcessDlqMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        const string queueName = "FakeQueueName";
        const string eventType = "FakeEventType";
        const string body = "FakeBody";
        var queueMessage = new QueueMessageBuilder()
            .WithMessageId(messageId)
            .WithCorrelationId(correlationId)
            .WithBody(body)
            .Build();
        var command = new ProcessDlqMessageCommandBuilder()
            .WithQueueMessage(queueMessage)
            .WithQueueName(queueName)
            .WithEventType(eventType)
            .Build();
        var builder = new ProcessDlqMessageHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        var alertService = builder.FreezeAlertService();
        var logger = builder.GetLogger();
        var sut = builder.Build();

        // Act
        await sut.Handle(command, CancellationToken.None);

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
            .SaveFailedMessageAsync(
                Arg.Is<FailedMessage>(message =>
                    message.MessageId == messageId &&
                    message.QueueName == queueName &&
                    message.EventType == eventType &&
                    message.CorrelationId == correlationId &&
                    message.Body == body));

        await alertService
            .Received(1)
            .SendAsync(
        Arg.Is<AlertRequest>(alert =>
                    alert.MessageId == messageId &&
                    alert.CorrelationId == correlationId &&
                    alert.Type == "ERROR" &&
                    alert.Error == "The message reached DLQ"));
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
            .SaveFailedMessageAsync(Arg.Any<FailedMessage>())
            .Throws(fakeException);
        
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(exceptionMessage);
        
        await alertService
            .DidNotReceive()
            .SendAsync(Arg.Any<AlertRequest>());
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
            .SendAsync(Arg.Any<AlertRequest>())
            .Throws(fakeException);
        
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(exceptionMessage);

        await failureStore
            .Received(1)
            .SaveFailedMessageAsync(Arg.Any<FailedMessage>());
    }
}