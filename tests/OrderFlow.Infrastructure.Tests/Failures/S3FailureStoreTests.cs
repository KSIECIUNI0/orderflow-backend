using FluentAssertions;
using OrderFlow.Infrastructure.TestUtils.Builders.Stores;

namespace OrderFlow.Infrastructure.Tests.Failures;

public class S3FailureStoreTests
{
    [Fact]
    public async Task SaveFailedMessageAsync_ShouldThrowAnArgumentNullException_WhenFailedMessageIsNull()
    {
        // Arrange
        var sut = new S3FailureStoreBuilder().Build();
        
        // Act
        var act = () => sut.SaveFailedMessageAsync(null!, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }
}