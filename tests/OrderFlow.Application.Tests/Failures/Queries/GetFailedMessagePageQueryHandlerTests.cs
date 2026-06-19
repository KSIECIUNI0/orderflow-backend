using NSubstitute;
using OrderFlow.Application.TestUtils.Builders.Handlers;
using OrderFlow.Application.TestUtils.Builders.Queries;

namespace OrderFlow.Application.Tests.Failures.Queries;

public class GetFailedMessagePageQueryHandlerTests
{
    [Theory]
    [InlineData("abcd1234")]
    [InlineData(null)]
    public async Task Handle_ShouldCallFailureStoreWithProperlyParams(string? continuationToken)
    {
        // Arrange
        const int pageSize = 20;
        var query = new GetFailedMessagePageQueryBuilder()
            .WithPageSize(pageSize)
            .WithContinuationToken(continuationToken)
            .Build();
        var builder = new GetFailedMessagePageQueryHandlerBuilder();
        var failureStore = builder.FreezeFailureStore();
        var sut = builder.Build();
        
        // Act
        await sut.Handle(query, TestContext.Current.CancellationToken);
        
        // Assert
        await failureStore
            .Received(1)
            .GetPageAsync(
                Arg.Is<int>(ps => ps == pageSize),
                Arg.Is<string>(ct => ct == continuationToken),
                Arg.Any<CancellationToken>());
    }
}