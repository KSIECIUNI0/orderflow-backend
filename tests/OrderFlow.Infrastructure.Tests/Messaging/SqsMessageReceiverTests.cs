using FluentAssertions;
using OrderFlow.Infrastructure.TestUtils.Builders.Messaging;

namespace OrderFlow.Infrastructure.Tests.Messaging;

public class SqsMessageReceiverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReceiveAsync_ShouldThrowAnArgumentException_WhenQueueUrlIsNullOrEmpty(string? queueUrl)
    {
        var sut = new SqsMessageReceiverBuilder().Build();

        var act = () => sut.ReceiveAsync(queueUrl!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("queueUrl");
    }

    [Fact]
    public async Task CompleteAsync_ShouldThrowAnArgumentNullException_WhenMessageIsNull()
    {
        var sut = new SqsMessageReceiverBuilder().Build();

        var act = () => sut.CompleteAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }
}