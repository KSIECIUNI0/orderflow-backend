using FluentAssertions;
using OrderFlow.Worker.Infrastructure;

namespace OrderFlow.Worker.Tests.Infrastructure;

public class AsyncCorrelationIdAccessorTests
{
    [Fact]
    public void Set_ShouldStoreCorrelationId()
    {
        // Arrange
        const string correlationId = "abc123";
        var sut = new AsyncCorrelationIdAccessor();

        // Act
        sut.Set(correlationId);

        // Assert
        sut.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public void Set_ShouldOverwritePreviousValue()
    {
        // Arrange
        var sut = new AsyncCorrelationIdAccessor();

        // Act
        sut.Set("first");
        sut.Set("second");

        // Assert
        sut.CorrelationId.Should().Be("second");
    }
    
    [Fact]
    public async Task CorrelationId_ShouldFlowThroughAwait()
    {
        // Arrange
        const string correlationId = "abc123";
        var sut = new AsyncCorrelationIdAccessor();

        sut.Set(correlationId);

        // Act
        await Task.Delay(1, TestContext.Current.CancellationToken);

        // Assert
        sut.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public async Task CorrelationId_ShouldBeIsolatedBetweenAsyncFlows()
    {
        // Arrange
        var sut = new AsyncCorrelationIdAccessor();

        async Task<string?> RunAsync(string correlationId)
        {
            sut.Set(correlationId);

            await Task.Delay(10);

            return sut.CorrelationId;
        }

        //Act
        var task1 = RunAsync("A");
        var task2 = RunAsync("B");

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Should().BeEquivalentTo("A", "B");
    }
}