namespace OrderFlow.Application.Orders.DTOs;

public record OrderResponse(
    Guid Id,
    string ProductName,
    int Quantity);