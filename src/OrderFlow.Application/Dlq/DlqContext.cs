namespace OrderFlow.Application.Dlq;

public record DlqContext(
    string QueueIdentifier,
    string EventType);