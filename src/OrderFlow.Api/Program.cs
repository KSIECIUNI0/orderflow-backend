using Amazon.Runtime;
using Amazon.SQS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Middleware;
using OrderFlow.Application.Common.EventBus;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;
using OrderFlow.Domain.Abstractions;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure.EventBus;
using OrderFlow.Infrastructure.Orders;
using OrderFlow.Infrastructure.Persistence;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig.GetValue<string>("ServiceUrl");
    var accessKey = awsConfig.GetValue<string>("AccessKey");
    var secretKey = awsConfig.GetValue<string>("SecretKey");
    
    var config = new AmazonSQSConfig
    {
        ServiceURL = serviceUrl,
        UseHttp = true
    };
    return new AmazonSQSClient(new BasicAWSCredentials(accessKey, secretKey), config);
});
builder.Services.AddSingleton<IEventBus, SqsEventBus>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S1118:Utility classes should not have public constructors",
    Justification = "Required for WebApplicationFactory test host discovery.")]
public partial class Program { }