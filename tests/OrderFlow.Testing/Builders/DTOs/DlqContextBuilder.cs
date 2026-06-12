using OrderFlow.Application.Dlq;

namespace OrderFlow.Testing.Builders.DTOs;

public class DlqContextBuilder
{
    private string _queueIdentifier = "created";
    private string _eventType = "fake event type";

    public DlqContextBuilder WithQueueIdentifier(string value)
    {
        _queueIdentifier = value;
        return this;
    }

    public DlqContextBuilder WithEventType(string value)
    {
        _eventType = value;
        return this;
    }
    
    public DlqContext Build() =>  new(_queueIdentifier, _eventType);
}