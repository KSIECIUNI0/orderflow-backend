using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.IntegrationTests.Factories;
using OrderFlow.IntegrationTests.Fixtures;

namespace OrderFlow.IntegrationTests.Controllers;

[Collection("Database collection")]
public class OrdersControllerTests
{
    private readonly HttpClient _client;

    public OrdersControllerTests(PostgreSqlContainerFixture fixture)
    {
        var factory = new OrderFlowApiFactory(fixture.ConnectionString);
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateOrder_Then_GetOrderById_ReturnsOrder()
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
    public async Task CreateOrder_WithInvalidModel_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest("", -1);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.Should()
            .Be400BadRequest()
            .And.HaveError("ProductName", "Product name is required")
            .And.HaveError("Quantity", "Quantity must be greater than 0");
    }
}