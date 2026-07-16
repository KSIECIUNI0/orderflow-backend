using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Orders;

public class OrderRepository(OrderFlowDbContext db) : IOrderRepository
{
    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await db.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Orders.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    public async Task<Order?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Update(Order order)
    {
        db.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        db.Orders.Remove(order);
    }
}