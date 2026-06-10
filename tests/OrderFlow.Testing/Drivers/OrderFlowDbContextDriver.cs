using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure;

namespace OrderFlow.Testing.Drivers;

public class OrderFlowDbContextDriver(OrderFlowDbContext dbContext)
{
    public async Task<Guid> InsertAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        var order = new Order(productName, quantity);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
    
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders.CountAsync(cancellationToken);
    }
    
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Orders\" RESTART IDENTITY;", cancellationToken);
    }
}