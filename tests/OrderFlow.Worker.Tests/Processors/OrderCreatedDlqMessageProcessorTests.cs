using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.Dlq.Commands;
using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Application.TestUtils.Builders.Messaging;
using OrderFlow.TestCore.Fakes;
using OrderFlow.Worker.TestUtils.Builders.Processors;

namespace OrderFlow.Worker.Tests.Processors;

public class OrderCreatedDlqMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldHandleAndCompleteMessage_WhenQueueMessageIsValid()
    {
        // Arrange
        var builder = new OrderCreatedDlqMessageProcessorBuilder();
        var mediator = builder.FreezeMediator();
        var messageReceiver = builder.FreezeMessageReceiver();
        var sut = builder.Build();
        var message = new QueueMessageBuilder().Build();

        const string expectedEventType = nameof(OrderCreatedEvent);
        const string expectedQueueId = QueueIdentifiers.OrderCreated;
        
        // Act
        await sut.ProcessAsync(message, TestContext.Current.CancellationToken);
        
        // Assert
        await mediator
            .Received(1)
            .Send(Arg.Is<ProcessDlqMessageCommand>(command =>
                command.QueueMessage == message &&
                command.Context.EventType == expectedEventType &&
                command.Context.QueueIdentifier == expectedQueueId),
                Arg.Any<CancellationToken>());
        
        await messageReceiver
            .Received(1)
            .CompleteAsync(message, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ProcessAsync_ShouldNotCompleteMessage_WhenMediatorThrowAnException()
    {
        // Arrange
        var builder = new OrderCreatedDlqMessageProcessorBuilder();
        var mediator = builder.FreezeMediator();
        var messageReceiver = builder.FreezeMessageReceiver();

        mediator
            .Send(
                Arg.Any<ProcessDlqMessageCommand>(),
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