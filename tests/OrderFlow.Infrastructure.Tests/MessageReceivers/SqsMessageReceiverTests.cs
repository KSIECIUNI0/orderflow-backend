using FluentAssertions;
using OrderFlow.Testing.Builders.Messaging;

namespace OrderFlow.Infrastructure.Tests.MessageReceivers;

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

    [Theory]
    [InlineData(null, "receiptHandle", "queueUrl")]
    [InlineData("", "receiptHandle", "queueUrl")]
    [InlineData("queueUrl", null, "receiptHandle")]
    [InlineData("queueUrl", "", "receiptHandle")]
    public async Task DeleteAsync_ShouldThrowAnArgumentException_WhenParametersAreNullOrEmpty(
        string? queueUrl,
        string? receiptHandle,
        string parameterName)
    {
        var sut = new SqsMessageReceiverBuilder().Build();

        var act = () => sut.DeleteAsync(queueUrl!, receiptHandle!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(parameterName);
    }
}