using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Infrastructure.Persistence;

public class UnitOfWork(OrderFlowDbContext orderFlowDbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await orderFlowDbContext.SaveChangesAsync(cancellationToken);
    }
}