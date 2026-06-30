using OrderFlow.Application.Messaging.DTOs;

namespace OrderFlow.Application.TestUtils.Builders.DTOs;

public class QueueMessageBuilder
{
    private string _messageId = "message id";
    private string _body = "message body";
    private string _correlationId = "correlation id";
    private string _queueUrl = "queue url";
    private const string ReceiptHandle = "receipt handle";

    public QueueMessageBuilder WithMessageId(string value)
    {
        _messageId = value;
        return this;
    }

    public QueueMessageBuilder WithBody(string value)
    {
        _body = value;
        return this;
    }

    public QueueMessageBuilder WithCorrelationId(string value)
    {
        _correlationId = value;
        return this;
    }

    public QueueMessageBuilder WithQueueUrl(string value)
    {
        _queueUrl = value;
        return this;
    }
    
    public QueueMessage Build() => new()
        {
            MessageId = _messageId,
            Body = _body,
            CorrelationId = _correlationId,
            ReceiptHandle = ReceiptHandle,
            QueueUrl = _queueUrl
        };
}