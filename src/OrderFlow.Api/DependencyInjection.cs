using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OrderFlow.Api.Configurations;
using OrderFlow.Api.Infrastructure;
using OrderFlow.Api.Middleware;
using OrderFlow.Application.Correlation;
using OrderFlow.Application.Metrics;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace OrderFlow.Api;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<OrderFlowMetricsOptions>()
            .BindConfiguration(OrderFlowMetricsOptions.SectionName)
            .Validate(
                OrderFlowMetricsOptions.HasValidBuckets,
                "OrderCreationDurationBuckets must contain positive, finite and strictly increasing values.")
            .ValidateOnStart();
        
        services.AddHttpContextAccessor();
        services.AddOpenApi();
        services.AddControllers();
        
        services.AddSingleton<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();
        
        services.AddTransient<ExceptionHandlingMiddleware>();
        
        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableBuiltInModelValidation = true;
        });

        var metricsOptions = GetMetricsOptions(configuration);
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService("orderflow-api"))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddPrometheusExporter()
                    .AddMeter(OrderFlowMetrics.MeterName)
                    .AddView(
                        "orderflow.orders.creation.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = metricsOptions.OrderCreationDurationBuckets
                        });
            });

        return services;
    }

    private static OrderFlowMetricsOptions GetMetricsOptions(IConfiguration configuration)
    {
        var metricsOptions = configuration
            .GetSection(OrderFlowMetricsOptions.SectionName)
            .Get<OrderFlowMetricsOptions>() ?? new OrderFlowMetricsOptions();
        
        if (!OrderFlowMetricsOptions.HasValidBuckets(metricsOptions))
        {
            throw new OptionsValidationException(
                nameof(OrderFlowMetricsOptions),
                typeof(OrderFlowMetricsOptions),
                [
                    "OrderCreationDurationBuckets must contain positive, finite and strictly increasing values."
                ]);
        }
        
        return metricsOptions;
    }
}