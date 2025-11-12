using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly OrderFlowDbContext _db;

    public OrderRepository(OrderFlowDbContext db)
    {
        _db = db;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _db.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Orders.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Update(Order order)
    {
        _db.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        _db.Orders.Remove(order);
    }
}