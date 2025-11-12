namespace OrderFlow.Application.Orders.DTOs;

public record CreateOrderRequest(
    string ProductName,
    int Quantity);