using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Api.TestUtils.Factories;

public class OrderFlowApiFactory(
    OrderFlowApiDependencyBuilder dependencyBuilder,
    OrderFlowApiAppConfigurationBuilder appConfigurationBuilder) : WebApplicationFactory<Program>
{
    public TDependency GetDependency<TDependency>()  where TDependency : class
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDependency>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(dependencyBuilder.Build);
        builder.ConfigureAppConfiguration(appConfigurationBuilder.Build);
    }
}