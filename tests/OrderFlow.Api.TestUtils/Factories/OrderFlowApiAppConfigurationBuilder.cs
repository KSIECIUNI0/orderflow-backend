using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OrderFlow.Api.TestUtils.Factories;

public class OrderFlowApiAppConfigurationBuilder
{
    private readonly Dictionary<string, string?> _configs = new();

    public OrderFlowApiAppConfigurationBuilder With(string key, string? value)
    {
        _configs.Add(key, value);
        return this;
    }

    public void Build(WebHostBuilderContext _, IConfigurationBuilder configBuilder)
    {
        if (_configs.Count > 0)
        {
            configBuilder.AddInMemoryCollection(_configs);
        }
    }
}