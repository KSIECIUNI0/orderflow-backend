using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Failures.DTOs;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.Messaging.Identifiers;
using OrderFlow.Application.TestUtils.Builders.Commands;
using OrderFlow.Application.TestUtils.Builders.Failures;
using OrderFlow.Application.TestUtils.Builders.Handlers;
using OrderFlow.Domain.Exceptions;
using OrderFlow.TestCore.Fakes;

namespace OrderFlow.Application.Tests.Failures.Commands;

public class ResendFailedMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrowAFailedMessageNotFoundException_WhenFailedMessageDoesNotExists()
    {
        // Arrange
        const string queueIdentifier = QueueIdentifiers.OrderCreated;
        var messageId = $"{Guid.NewGuid()}";
        var command = new ResendFailedMessageCommandBuilder()
            .WithQueueIdentifier(queueIdentifier)
            .WithMessageId(messageId)
            .Build();
        
        var builder = new ResendFailedMessageCommandHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        failureStore
            .GetFailedMessageAsync(
                Arg.Is<string>(queueId => queueId == queueIdentifier),
                Arg.Is<string>(msgId => msgId == messageId),
                Arg.Any<CancellationToken>())
            .Returns(null as FailedMessage);
        
        var messagePublisher = builder.FreezeMessagePublisher();
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FailedMessageNotFoundException>()
            .WithMessage($"Failed message {messageId} was not found");
        
        await messagePublisher
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<OutgoingMessage>(),
                Arg.Any<CancellationToken>());

        await failureStore
            .DidNotReceive()
            .DeleteFailedMessageAsync(
                Arg.Is<string>(queueId => queueId == queueIdentifier),
                Arg.Is<string>(msgId => msgId == messageId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowAnExceptionAndBreakHandling_WhenGetFailedMessageAsyncThrowAnException()
    {
        // Arrange
        const string errorMessage = "fake exception occured";
        var exception = new FakeException(errorMessage);
        var command = new ResendFailedMessageCommandBuilder().Build();
        var builder = new ResendFailedMessageCommandHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        failureStore
            .GetFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        
        var messagePublisher = builder.FreezeMessagePublisher();
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(errorMessage);
        
        await messagePublisher
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<OutgoingMessage>(),
                Arg.Any<CancellationToken>());

        await failureStore
            .DidNotReceive()
            .DeleteFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnExceptionAndBreakHandling_WhenPublishAsyncThrowAnException()
    {
        // Arrange
        const string errorMessage = "fake exception occured";
        var exception = new FakeException(errorMessage);
        var failedMessage = new FailedMessageBuilder().Build();
        var command = new ResendFailedMessageCommandBuilder().Build();
        var builder = new ResendFailedMessageCommandHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        failureStore
            .GetFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(failedMessage);
        
        var messagePublisher = builder.FreezeMessagePublisher();
        messagePublisher
            .PublishAsync(
                Arg.Any<OutgoingMessage>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(errorMessage);

        await failureStore
            .Received(1)
            .GetFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await failureStore
            .DidNotReceive()
            .DeleteFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowAnException_WhenDeleteFailedMessageAsyncThrowAnException()
    {
        // Arrange
        const string errorMessage = "fake exception occured";
        var exception = new FakeException(errorMessage);
        var failedMessage = new FailedMessageBuilder().Build();
        var command = new ResendFailedMessageCommandBuilder().Build();
        var builder = new ResendFailedMessageCommandHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        failureStore
            .GetFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(failedMessage);
        
        failureStore
            .DeleteFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        
        var messagePublisher = builder.FreezeMessagePublisher();
        var sut = builder.Build();
        
        // Act
        var act = () => sut.Handle(command, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<FakeException>()
            .WithMessage(errorMessage);

        await failureStore
            .Received(1)
            .GetFailedMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await messagePublisher
            .Received(1)
            .PublishAsync(
                Arg.Any<OutgoingMessage>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldResendFailedMessage()
    {
        // Arrange
        const string queueIdentifier = QueueIdentifiers.OrderCreated;
        const string failedMessageBody = "failed_message_body";
        var messageId = $"{Guid.NewGuid()}";
        var correlationId = $"{Guid.NewGuid()}";
        
        var failedMessage = new FailedMessageBuilder()
            .WithBody(failedMessageBody)
            .WithCorrelationId(correlationId)
            .Build();
        var command = new ResendFailedMessageCommandBuilder()
            .WithQueueIdentifier(queueIdentifier)
            .WithMessageId(messageId)
            .Build();
        var builder = new ResendFailedMessageCommandHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        failureStore
            .GetFailedMessageAsync(
                Arg.Is<string>(queueOd => queueOd == queueIdentifier),
                Arg.Is<string>(msgId => msgId == messageId),
                Arg.Any<CancellationToken>())
            .Returns(failedMessage);
        
        var messagePublisher = builder.FreezeMessagePublisher();
        var sut = builder.Build();
        
        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await failureStore
            .Received(1)
            .GetFailedMessageAsync(
                Arg.Is<string>(queueOd => queueOd == queueIdentifier),
                Arg.Is<string>(msgId => msgId == messageId),
                Arg.Any<CancellationToken>());
        
        await messagePublisher
            .Received(1)
            .PublishAsync(
                Arg.Is<OutgoingMessage>(outgoingMsg => 
                    outgoingMsg.Body == failedMessageBody &&
                    outgoingMsg.CorrelationId == correlationId &&
                    outgoingMsg.QueueIdentifier == queueIdentifier),
                Arg.Any<CancellationToken>());
        
        await failureStore
            .Received(1)
            .DeleteFailedMessageAsync(
                Arg.Is<string>(queueOd => queueOd == queueIdentifier),
                Arg.Is<string>(msgId => msgId == messageId),
                Arg.Any<CancellationToken>());
    }
}