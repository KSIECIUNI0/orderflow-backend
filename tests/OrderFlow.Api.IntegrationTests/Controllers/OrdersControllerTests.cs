using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrderFlow.Api.IntegrationTests.Factories;
using OrderFlow.Api.IntegrationTests.Fixtures;
using OrderFlow.Api.IntegrationTests.Helpers;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Testing.Builders.DTOs;

namespace OrderFlow.Api.IntegrationTests.Controllers;

[Collection("Integration tests collection")]
public class OrdersControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly OrderDataHelper _orderDataHelper;
    private readonly SqsHelper _sqsHelper;
    private readonly string _queueUrl;

    public OrdersControllerTests(
        PostgreSqlContainerFixture dbFixture,
        LocalStackContainerFixture awsFixture)
    {
        var factory = new OrderFlowApiFactory(
            dbFixture.ConnectionString,
            awsFixture.SqsUrl);
        _client = factory.CreateClient();
        _orderDataHelper = factory.OrderDataHelper;
        _sqsHelper = factory.SqsHelper;
        _queueUrl = awsFixture.QueueUrl;
    }

    public async ValueTask InitializeAsync()
    {
        await _orderDataHelper.ClearAsync();
        await _sqsHelper.ClearAsync(_queueUrl);
    }

    public ValueTask DisposeAsync() 
        => ValueTask.CompletedTask;
    
    [Fact]
    public async Task CreateOrder_ShouldCreateOrder_WhenModelIsValid()
    {
        // Arrange
        const string productName = "product name";
        const int quantity = 1;
        var request = new CreateOrderRequestBuilder()
            .WithProductName(productName)
            .WithQuantity(quantity)
            .Build();
        
        // Act
        var createdResponse = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        createdResponse.Should()
            .Be201Created()
            .And.HaveHeader("Location").And.NotBeEmpty()
            .And.HaveHeader("X-Correlation-ID").And.NotBeEmpty();
        
        var correlationId = createdResponse.Headers.GetValues("X-Correlation-ID").First();
        
        var createdOrderResponse = await createdResponse.Content.ReadFromJsonAsync<OrderResponse>(TestContext.Current.CancellationToken);
        createdOrderResponse.Should().NotBeNull();
        createdOrderResponse.Id.Should().NotBeEmpty();
        createdOrderResponse.ProductName.Should().Be(productName);
        createdOrderResponse.Quantity.Should().Be(quantity);

        var getResponse = await _client.GetAsync($"/api/orders/{createdOrderResponse.Id}", TestContext.Current.CancellationToken);
        
        getResponse.Should().Be200Ok();
        var orderResponse = await getResponse.Content.ReadFromJsonAsync<OrderResponse>(TestContext.Current.CancellationToken);
        orderResponse.Should().NotBeNull();
        orderResponse.Id.Should().Be(createdOrderResponse.Id);
        orderResponse.ProductName.Should().Be(createdOrderResponse.ProductName);
        orderResponse.Quantity.Should().Be(createdOrderResponse.Quantity);

        var messages = await _sqsHelper.ReadAllAsync(_queueUrl, TestContext.Current.CancellationToken);
        messages.Should().NotBeNullOrEmpty();
        messages.Should().HaveCount(1);
        var message = messages.First();
        
        var messageCorrelationId = message.MessageAttributes.TryGetValue("CorrelationId", out var attr)
            ? attr.StringValue
            : null;
        messageCorrelationId.Should().NotBeNullOrEmpty();
        messageCorrelationId.Should().Be(correlationId);
        
        var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
        @event.Should().NotBeNull();
        @event.OrderId.Should().Be(createdOrderResponse.Id);
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenCallWithInvalidModel()
    {
        var request = new CreateOrderRequestBuilder()
            .WithEmptyProductName()
            .WithNegativeQuantity()
            .Build();

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.Should()
            .Be400BadRequest()
            .And.HaveError("ProductName", "Product name is required")
            .And.HaveError("Quantity", "Quantity must be greater than 0");
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenCallWithNotExistingId()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        var response  = await _client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);
        
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllOrders()
    {
        // Arrange
        const string order1ProductName = "product name 1";
        const int order1Quantity = 1;
        const string order2ProductName = "product name 2";
        const int order2Quantity = 2;
        var order1Id = await _orderDataHelper.InsertAsync(order1ProductName, order1Quantity);
        var order2Id = await _orderDataHelper.InsertAsync(order2ProductName, order2Quantity);
        
        // Act
        var response = await _client.GetAsync("api/orders", TestContext.Current.CancellationToken);
        
        // Assert
        response.Should()
            .Be200Ok()
            .And.Satisfy<IEnumerable<OrderResponse>>(model => 
                model.Should()
                    .HaveCount(2)
                    .And.SatisfyRespectively( first =>
                    {
                        first.Id.Should().Be(order1Id);
                        first.ProductName.Should().Be(order1ProductName);
                        first.Quantity.Should().Be(order1Quantity);
                    },
                    second =>
                    {
                        second.Id.Should().Be(order2Id);
                        second.ProductName.Should().Be(order2ProductName);
                        second.Quantity.Should().Be(order2Quantity);
                    })
            );
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyCollection_WhenThereAreNoOrders()
    {
        var orderCount = await _orderDataHelper.CountAsync();

        orderCount.Should().Be(0);
        
        var response = await _client.GetAsync("api/orders", TestContext.Current.CancellationToken);

        response.Should()
            .Be200Ok()
            .And.Satisfy<IEnumerable<OrderResponse>>(model =>
                model.Should().BeEmpty());
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenCallWithInvalidModel()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var request = new CreateOrderRequestBuilder()
            .WithEmptyProductName()
            .WithNegativeQuantity()
            .Build();
        
        var response  = await _client.PutAsJsonAsync($"/api/orders/{orderId}", request);
        
        response.Should()
            .Be400BadRequest()
            .And.HaveError("ProductName", "Product name is required")
            .And.HaveError("Quantity", "Quantity must be greater than 0");
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNotFound_WhenCallWithNotExistingId()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var request = new CreateOrderRequestBuilder().Build();
        
        var response  = await _client.PutAsJsonAsync($"/api/orders/{orderId}", request);

        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateOrder_WhenModelIsValid()
    {
        const string newProductName = "New product name";
        const int newQuantity = 100;
        
        var orderId = await _orderDataHelper.InsertAsync("productName", 1);
        
        var request = new CreateOrderRequestBuilder()
            .WithProductName(newProductName)
            .WithQuantity(newQuantity)
            .Build();
        
        var updateResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}", request);

        updateResponse.Should().Be204NoContent();
        
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);
        
        getResponse.Should().Be200Ok();
        var orderResponse = await getResponse.Content.ReadFromJsonAsync<OrderResponse>(TestContext.Current.CancellationToken);
        
        orderResponse!.ProductName.Should().Be(newProductName);
        orderResponse.Quantity.Should().Be(newQuantity);
    }
    
    [Fact]
    public async Task DeleteOrder_ShouldReturnNotFound_WhenCallWithNotExistingId()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        var response  = await _client.DeleteAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);

        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task DeleteOrder_ShouldDeleteOrder_WhenExists()
    {
        var orderId = await _orderDataHelper.InsertAsync("productName", 1);
        
        var deleteResponse = await _client.DeleteAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);

        deleteResponse.Should().Be204NoContent();
        
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);
        
        getResponse.Should().Be404NotFound();
    }
}