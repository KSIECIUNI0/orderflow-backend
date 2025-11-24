using Amazon.Runtime;
using Amazon.SQS;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Infrastructure;
using OrderFlow.Api.Middleware;
using OrderFlow.Application.Common.Behaviors;
using OrderFlow.Application.Common.EventBus;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure.EventBus;
using OrderFlow.Infrastructure.Orders;
using OrderFlow.Infrastructure.Persistence;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommandHandler>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableBuiltInModelValidation = true;
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();
builder.Services.AddSingleton<IEventBus, SqsEventBus>();
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
    var accessKey = awsConfig.GetValue<string>("AccessKey");
    var secretKey = awsConfig.GetValue<string>("SecretKey");

    return new AmazonSQSClient(
        new BasicAWSCredentials(accessKey, secretKey),
        new AmazonSQSConfig
        {
            ServiceURL = serviceUrl,
            UseHttp = true
        });
});

builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationIdBehavior<,>));
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapControllers();

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S1118:Utility classes should not have public constructors",
    Justification = "Required for WebApplicationFactory test host discovery.")]
public partial class Program { }