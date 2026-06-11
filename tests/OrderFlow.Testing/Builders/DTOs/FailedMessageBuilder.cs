using OrderFlow.Application.Failures;

namespace OrderFlow.Testing.Builders.DTOs;

public class FailedMessageBuilder
{
    private string _messageId = "fake message id";
    private string _queueName = "fake queue name";
    private string _correlationId = "fake correlation id";
    private string _eventType = "fake event type";
    private string _body = "fake body";

    public FailedMessageBuilder WithMessageId(string value)
    {
        _messageId = value;
        return this;
    }

    public FailedMessageBuilder WithQueueName(string value)
    {
        _queueName = value;
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

    public FailedMessage Build()
    {
        return new FailedMessage
        {
            MessageId = _messageId,
            QueueName = _queueName,
            CorrelationId = _correlationId,
            EventType = _eventType,
            Body = _body
        };
    }
}