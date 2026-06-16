using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Testing.Builders.DTOs;
using OrderFlow.Testing.Builders.Processors;
using OrderFlow.Testing.Fakes;

namespace OrderFlow.Worker.Tests.Processors;

public class OrderCreatedMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldHandleAndCompleteMessage_WhenQueueMessageIsValid()
    {
        // Arrange 
        var builder = new OrderCreatedMessageProcessorBuilder();
        var mediator = builder.FreezeMediator();
        var messageReceiver = builder.FreezeMessageReceiver();
        var sut = builder.Build();

        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var @event = new OrderCreatedEvent(orderId);
        var message = new QueueMessageBuilder()
            .WithBody(JsonSerializer.Serialize(@event))
            .Build();
        
        // Act
        await sut.ProcessAsync(message, TestContext.Current.CancellationToken);
        
        // Assert
        await mediator
            .Received(1)
            .Send(
                Arg.Is<ProcessOrderCreatedCommand>(x => x.OrderId == orderId),
                Arg.Any<CancellationToken>());
        
        await messageReceiver
            .Received(1)
            .CompleteAsync(message, Arg.Any<CancellationToken>());
    }
    
    [Theory]
    [InlineData("invalid-json")]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData(
        """
        {
            "orderId":"00000000-0000-0000-0000-000000000000"
        }
        """)]
    [InlineData("null")]
    public async Task ProcessAsync_ShouldCatchAnExceptionAndNotProcessMessage_WhenMessageBodyIsInvalid(
        string invalidBody)
    {
        // Arrange
        var builder = new OrderCreatedMessageProcessorBuilder();
        var mediator = builder.FreezeMediator();
        var messageReceiver = builder.FreezeMessageReceiver();
        var correlationIdAccessor = builder.FreezeCorrelationIdAccessor();
        var logger = builder.GetLogger();

        const string messageId = "abcd1234";
        const string correlationId = "123456";
        var message = new QueueMessageBuilder()
            .WithMessageId(messageId)
            .WithCorrelationId(correlationId)
            .WithBody(invalidBody)
            .Build();

        correlationIdAccessor
            .CorrelationId
            .Returns(correlationId);
        
        var sut = builder.Build();
        
        // Act
        await sut.ProcessAsync(message, TestContext.Current.CancellationToken);
        
        // Assert
        await mediator
            .DidNotReceive()
            .Send(
                Arg.Any<ProcessOrderCreatedCommand>(),
                Arg.Any<CancellationToken>());
        
        await messageReceiver
            .DidNotReceive()
            .CompleteAsync(
                Arg.Any<QueueMessage>(),
                Arg.Any<CancellationToken>());
        
        var log = logger.Collector.GetSnapshot()
            .FirstOrDefault(l => l.Level == LogLevel.Error);

        log.Should().NotBeNull();
        log.Exception.Should().NotBeNull();
        log.State.Should().NotBeNull();
        var logState = log.State
            .Should()
            .BeAssignableTo<IEnumerable<KeyValuePair<string, string>>>()
            .Subject;
        logState
            .Should()
            .ContainKeys("MessageId", "CorrelationId")
            .And
            .ContainValues(messageId, correlationId);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotCompleteMessage_WhenMediatorThrowAnException()
    {
        // Arrange
        var builder = new OrderCreatedMessageProcessorBuilder();
        var mediator = builder.FreezeMediator();
        var messageReceiver = builder.FreezeMessageReceiver();

        mediator
            .Send(
                Arg.Any<ProcessOrderCreatedCommand>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new FakeException("Fake mediator exception"));
        
        var message = new QueueMessageBuilder().Build();
        var sut = builder.Build();
        
        // Act
        await sut.ProcessAsync(message, TestContext.Current.CancellationToken);
        
        // Assert
        await messageReceiver
            .DidNotReceive()
            .CompleteAsync(
                Arg.Any<QueueMessage>(),
                Arg.Any<CancellationToken>());
    }
}