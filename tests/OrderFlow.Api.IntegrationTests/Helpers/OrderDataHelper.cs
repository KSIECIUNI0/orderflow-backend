using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure;

namespace OrderFlow.Api.IntegrationTests.Helpers;

public class OrderDataHelper
{
    private readonly IServiceProvider _services;

    public OrderDataHelper(IServiceProvider services)
    {
        _services = services;
    }
    
    public async Task<Guid> InsertAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        var order = new Order(productName, quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
    
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        return await db.Orders.CountAsync(cancellationToken);
    }
    
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
        
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Orders\" RESTART IDENTITY;", cancellationToken);
    }
}