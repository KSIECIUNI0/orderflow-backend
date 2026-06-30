using OrderFlow.Application.Messaging.DTOs;

namespace OrderFlow.Application.TestUtils.Builders.DTOs;

public class OutgoingMessageBuilder
{
    private string _queueIdentifier = "queue_identifier";
    private string _body = "body";
    private string? _correlationId = "correlation_id";

    public OutgoingMessageBuilder WithQueueIdentifier(string value)
    {
        _queueIdentifier = value;
        return this;
    }

    public OutgoingMessageBuilder WithBody(string value)
    {
        _body = value;
        return this;
    }

    public OutgoingMessageBuilder WithCorrelationId(string value)
    {
        _correlationId = value;
        return this;
    }
    
    public OutgoingMessageBuilder WithoutCorrelationId()
    {
        _correlationId = null;
        return this;
    }

    public OutgoingMessage Build()
        => new()
        {
            QueueIdentifier = _queueIdentifier,
            Body = _body,
            CorrelationId = _correlationId,
        };
}