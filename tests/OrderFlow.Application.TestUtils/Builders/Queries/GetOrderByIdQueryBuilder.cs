using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Application.TestUtils.Builders.Queries;

public class GetOrderByIdQueryBuilder
{
    private Guid _id = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public GetOrderByIdQueryBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }

    public GetOrderByIdQuery Build() => new(_id);
}