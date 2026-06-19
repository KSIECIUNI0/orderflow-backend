using OrderFlow.Application.Failures;
using OrderFlow.Application.Failures.DTOs;

namespace OrderFlow.Application.TestUtils.Builders.Failures;

public class FailedMessageBuilder
{
    private string _messageId = "fake message id";
    private string _queueIdentifier = "fake queue id";
    private string _correlationId = "fake correlation id";
    private string _eventType = "fake event type";
    private string _body = "fake body";

    public FailedMessageBuilder WithMessageId(string value)
    {
        _messageId = value;
        return this;
    }

    public FailedMessageBuilder WithQueueIdentifier(string value)
    {
        _queueIdentifier = value;
        return this;
    }

    public FailedMessageBuilder WithCorrelationId(string value)
    {
        _correlationId = value;
        return this;
    }

    public FailedMessageBuilder WithEventType(string value)
    {
        _eventType = value;
        return this;
    }

    public FailedMessageBuilder WithBody(string value)
    {
        _body = value;
        return this;
    }

    public FailedMessage Build() => new()
        {
            MessageId = _messageId,
            QueueIdentifier = _queueIdentifier,
            CorrelationId = _correlationId,
            EventType = _eventType,
            Body = _body
        };
}