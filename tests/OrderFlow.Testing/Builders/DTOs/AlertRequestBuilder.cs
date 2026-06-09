using OrderFlow.Application.Alerts;

namespace OrderFlow.Testing.Builders.DTOs;

public class AlertRequestBuilder
{
    private string _type = "FakeType";
    private string _correlationId = "FakeCorrelationId";
    private string _error = "FakeError";
    private string _messageId = "FakeMessageId";

    public AlertRequestBuilder WithType(string value)
    {
        _type = value;
        return this;
    }

    public AlertRequestBuilder WithCorrelationId(string value)
    {
        _correlationId = value;
        return this;
    }

    public AlertRequestBuilder WithError(string value)
    {
        _error = value;
        return this;
    }

    public AlertRequestBuilder WithMessageId(string value)
    {
        _messageId = value;
        return this;
    }

    public AlertRequest Build()
    {
        return new AlertRequest
        {
            Type = _type,
            CorrelationId = _correlationId,
            Error = _error,
            MessageId = _messageId
        };
    }
}