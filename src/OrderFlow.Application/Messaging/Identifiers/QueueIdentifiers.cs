namespace OrderFlow.Application.Messaging.Identifiers;

public static class QueueIdentifiers
{
    public const string OrderCreated = "order-created";
    public const string OrderCreatedDlq = "order-created-dlq";
}