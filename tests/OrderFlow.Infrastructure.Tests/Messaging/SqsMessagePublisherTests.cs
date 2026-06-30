using FluentAssertions;
using OrderFlow.Infrastructure.TestUtils.Builders.Messaging;

namespace OrderFlow.Infrastructure.Tests.Messaging;

public class SqsMessagePublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldThrowAnArgumentNullException_WhenOutgoingMessageIsNull()
    {
        // Arrange
        var sut = new SqsMessagePublisherBuilder().Build();
        
        // Act
        var act = () => sut.PublishAsync(null!, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }
}