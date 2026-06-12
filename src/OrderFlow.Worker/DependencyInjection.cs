using System.Diagnostics.CodeAnalysis;
using OrderFlow.Application.Correlation;
using OrderFlow.Worker.Infrastructure;
using OrderFlow.Worker.Processors;
using OrderFlow.Worker.Workers;

namespace OrderFlow.Worker;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdAccessor, AsyncCorrelationIdAccessor>();
        services.AddScoped<OrderCreatedMessageProcessor>();
        services.AddScoped<OrderCreatedDlqMessageProcessor>();
        services.AddHostedService<OrderCreatedWorker>();
        services.AddHostedService<OrderCreatedDlqWorker>();
        
        return services;
    }
}