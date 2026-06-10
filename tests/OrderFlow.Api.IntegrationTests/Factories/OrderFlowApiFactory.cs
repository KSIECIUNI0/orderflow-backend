using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Infrastructure;

namespace OrderFlow.Api.IntegrationTests.Factories;

public class OrderFlowApiFactory(
    string dbConnectionString,
    string sqsUrl) : WebApplicationFactory<Program>
{
    public TDependency GetDependency<TDependency>()  where TDependency : class
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDependency>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<OrderFlowDbContext>));
            services.AddDbContext<OrderFlowDbContext>(options =>
            {
                options.UseNpgsql(dbConnectionString);
            });

            services.RemoveAll(typeof(IAmazonSQS));
            services.AddSingleton<IAmazonSQS>(_ =>
            {
                var config = new AmazonSQSConfig
                {
                    ServiceURL = sqsUrl,
                    UseHttp = true
                };
                return new AmazonSQSClient(
                    new BasicAWSCredentials("test", "test"),
                    config);
            });
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
            db.Database.Migrate();
        });
    }
}