using MediatR;

namespace OrderFlow.Application.Orders.Events;

public record OrderCreatedEvent(Guid OrderId) : INotification;