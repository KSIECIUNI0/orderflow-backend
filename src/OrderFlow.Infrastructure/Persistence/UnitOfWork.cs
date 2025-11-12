using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderFlowDbContext _orderFlowDbContext;

    public UnitOfWork(OrderFlowDbContext orderFlowDbContext)
    {
        _orderFlowDbContext = orderFlowDbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _orderFlowDbContext.SaveChangesAsync(cancellationToken);
    }
}