namespace OrderFlow.Application.Orders.Events;

public record OrderCreatedEvent(Guid OrderId)
{
    public void Validate() => 
        ArgumentOutOfRangeException.ThrowIfEqual(
            OrderId,
            Guid.Empty);
}