using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.IntegrationTests.Factories;
using OrderFlow.IntegrationTests.Fixtures;

namespace OrderFlow.IntegrationTests.Controllers;

[Collection("Database collection")]
public class OrdersControllerTests
{
    private readonly OrderFlowApiFactory _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(PostgreSqlContainerFixture fixture)
    {
        _factory = new OrderFlowApiFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateOrder_Then_GetOrderById_ShouldReturnOrder_WhenCallWithValidModel()
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
        var orderDataHelper = _factory.OrderDataHelper;
        
        const string order1ProductName = "product name 1";
        const int order1Quantity = 1;
        const string order2ProductName = "product name 2";
        const int order2Quantity = 2;
        var order1Id = await orderDataHelper.InsertAsync(order1ProductName, order1Quantity);
        var order2Id = await orderDataHelper.InsertAsync(order2ProductName, order2Quantity);
        
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
        var orderDataHelper = _factory.OrderDataHelper;
        var orderCount = await orderDataHelper.CountAsync();

        orderCount.Should().Be(0);
        
        var response = await _client.GetAsync("api/orders");

        response.Should()
            .Be200Ok()
            .And.Satisfy<IEnumerable<OrderResponse>>(model =>
                model.Should().BeEmpty());
    }
}