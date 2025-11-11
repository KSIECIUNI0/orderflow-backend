using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.IntegrationTests.Factories;
using OrderFlow.IntegrationTests.Fixtures;
using OrderFlow.IntegrationTests.Helpers;

namespace OrderFlow.IntegrationTests.Controllers;

[Collection("Database collection")]
public class OrdersControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly OrderDataHelper _orderDataHelper;

    public OrdersControllerTests(PostgreSqlContainerFixture fixture)
    {
        var factory = new OrderFlowApiFactory(fixture.ConnectionString);
        _client = factory.CreateClient();
        _orderDataHelper = factory.OrderDataHelper;
    }
    
    public async Task InitializeAsync()
        => await _orderDataHelper.ClearAsync();

    public Task DisposeAsync() 
        => Task.CompletedTask;
    
    [Fact]
    public async Task CreateOrder_ShouldCreateOrder_WhenModelIsValid()
    {
        const string productName = "product name";
        const int quantity = 1;
        var request = new CreateOrderRequest(productName, quantity);
        
        var createdResponse = await _client.PostAsJsonAsync("/api/orders", request);

        createdResponse.Should()
            .Be201Created()
            .And.HaveHeader("Location").And.NotBeEmpty();
        
        var createdOrderResponse = await createdResponse.Content.ReadFromJsonAsync<OrderResponse>();
        createdOrderResponse.Should().NotBeNull();
        createdOrderResponse.Id.Should().NotBeEmpty();
        createdOrderResponse.ProductName.Should().Be(productName);
        createdOrderResponse.Quantity.Should().Be(quantity);

        var getResponse = await _client.GetAsync($"/api/orders/{createdOrderResponse.Id}");
        
        getResponse.Should().Be200Ok();
        var orderResponse = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Should().NotBeNull();
        orderResponse.Id.Should().Be(createdOrderResponse.Id);
        orderResponse.ProductName.Should().Be(createdOrderResponse.ProductName);
        orderResponse.Quantity.Should().Be(createdOrderResponse.Quantity);
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenCallWithInvalidModel()
    {
        var request = new CreateOrderRequest("", -1);

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
        
        var response  = await _client.GetAsync($"/api/orders/{orderId}");
        
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllOrders()
    {
        const string order1ProductName = "product name 1";
        const int order1Quantity = 1;
        const string order2ProductName = "product name 2";
        const int order2Quantity = 2;
        var order1Id = await _orderDataHelper.InsertAsync(order1ProductName, order1Quantity);
        var order2Id = await _orderDataHelper.InsertAsync(order2ProductName, order2Quantity);
        
        var response = await _client.GetAsync("api/orders");
        
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
        
        var response = await _client.GetAsync("api/orders");

        response.Should()
            .Be200Ok()
            .And.Satisfy<IEnumerable<OrderResponse>>(model =>
                model.Should().BeEmpty());
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenCallWithInvalidModel()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var request = new CreateOrderRequest("", -1);
        
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
        var request = new CreateOrderRequest("ProductName", 1);
        
        var response  = await _client.PutAsJsonAsync($"/api/orders/{orderId}", request);

        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateOrder_WhenModelIsValid()
    {
        const string newProductName = "New product name";
        const int newQuantity = 100;
        
        var orderId = await _orderDataHelper.InsertAsync("productName", 1);
        
        var request = new CreateOrderRequest(newProductName, newQuantity);
        
        var updateResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}", request);

        updateResponse.Should().Be204NoContent();
        
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        
        getResponse.Should().Be200Ok();
        var orderResponse = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        
        orderResponse!.ProductName.Should().Be(newProductName);
        orderResponse.Quantity.Should().Be(newQuantity);
    }
    
    [Fact]
    public async Task DeleteOrder_ShouldReturnNotFound_WhenCallWithNotExistingId()
    {
        var orderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        var response  = await _client.DeleteAsync($"/api/orders/{orderId}");

        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task DeleteOrder_ShouldDeleteOrder_WhenExists()
    {
        var orderId = await _orderDataHelper.InsertAsync("productName", 1);
        
        var deleteResponse = await _client.DeleteAsync($"/api/orders/{orderId}");

        deleteResponse.Should().Be204NoContent();
        
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        
        getResponse.Should().Be404NotFound();
    }
}