using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Infrastructure;

namespace OrderFlow.IntegrationTests.Factories;

public class OrderFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public OrderFlowApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<OrderFlowDbContext>));
            services.AddDbContext<OrderFlowDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
            db.Database.Migrate();
        });
    }
}