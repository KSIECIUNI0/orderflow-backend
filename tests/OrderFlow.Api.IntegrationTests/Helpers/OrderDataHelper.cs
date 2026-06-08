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
    
    public async Task<Guid> InsertAsync(string productName, int quantity)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        var order = new Order(productName, quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
    }
    
    public async Task<int> CountAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        return await db.Orders.CountAsync();
    }
    
    public async Task ClearAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
        
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Orders\" RESTART IDENTITY;");
    }
}