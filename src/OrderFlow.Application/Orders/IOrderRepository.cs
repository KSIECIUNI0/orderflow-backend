using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
}