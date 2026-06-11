using FluentAssertions;
using OrderFlow.Testing.Builders.EventBus;

namespace OrderFlow.Infrastructure.Tests.EventBus;

public class SqsEventBusTests
{
    [Fact]
    public async Task PublishOrderCreatedAsync_ShouldThrowAnArgumentNullException_WhenOrderCreatedEventIsNull()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var sut = new SqsEventBusBuilder().Build();
        
        // Act
        var act = () => sut.PublishOrderCreatedAsync(null!, correlationId, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("event");
    }
}