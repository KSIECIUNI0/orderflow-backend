using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Infrastructure;
using OrderFlow.IntegrationTests.Helpers;

namespace OrderFlow.IntegrationTests.Factories;

public class OrderFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbConnectionString;
    private readonly string _sqsUrl;

    public OrderFlowApiFactory(
        string dbConnectionString,
        string sqsUrl)
    {
        _dbConnectionString = dbConnectionString;
        _sqsUrl = sqsUrl;
    }

    public OrderDataHelper OrderDataHelper => new(Services);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<OrderFlowDbContext>));
            services.AddDbContext<OrderFlowDbContext>(options =>
            {
                options.UseNpgsql(_dbConnectionString);
            });

            services.RemoveAll(typeof(IAmazonSQS));
            services.AddSingleton<IAmazonSQS>(_ =>
            {
                var config = new AmazonSQSConfig
                {
                    ServiceURL = _sqsUrl,
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