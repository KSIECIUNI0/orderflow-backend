using OrderFlow.Api.Infrastructure;
using OrderFlow.Api.Middleware;
using OrderFlow.Application.Correlation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace OrderFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddOpenApi();
        services.AddControllers();
        
        services.AddSingleton<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();
        
        services.AddTransient<ExceptionHandlingMiddleware>();
        
        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableBuiltInModelValidation = true;
        });

        return services;
    }
}