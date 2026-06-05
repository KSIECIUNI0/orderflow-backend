using OrderFlow.Application.Correlation;
using OrderFlow.Worker.Infrastructure;
using OrderFlow.Worker.Orders.Workers;

namespace OrderFlow.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdAccessor, AsyncCorrelationIdAccessor>();
        services.AddHostedService<OrderCreatedWorker>();
        services.AddHostedService<OrderCreatedDlqWorker>();
        
        return services;
    }
}