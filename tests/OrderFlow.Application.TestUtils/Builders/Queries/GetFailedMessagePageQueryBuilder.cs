using OrderFlow.Application.Failures.Queries;

namespace OrderFlow.Application.TestUtils.Builders.Queries;

public class GetFailedMessagePageQueryBuilder
{
    private int _pageSize = 10;
    private string? _continuationToken = null;

    public GetFailedMessagePageQueryBuilder WithPageSize(int value)
    {
        _pageSize = value;
        return this;
    }

    public GetFailedMessagePageQueryBuilder WithContinuationToken(string? value)
    {
        _continuationToken = value;
        return this;
    }
    
    public GetFailedMessagePageQuery Build() => new(_pageSize, _continuationToken);
}