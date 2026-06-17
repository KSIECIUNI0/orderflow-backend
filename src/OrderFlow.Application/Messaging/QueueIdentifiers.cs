namespace OrderFlow.Application.Messaging;

public static class QueueIdentifiers
{
    public const string OrderCreated = "order-created";
    public const string OrderCreatedDlq = "order-created-dlq";
}