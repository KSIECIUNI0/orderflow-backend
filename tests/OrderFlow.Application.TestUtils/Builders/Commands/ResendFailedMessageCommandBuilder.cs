using OrderFlow.Application.Failures.Commands;

namespace OrderFlow.Application.TestUtils.Builders.Commands;

public class ResendFailedMessageCommandBuilder
{
    private string _queueIdentifier = "queue_identifier";
    private string _messageId = "message_id";

    public ResendFailedMessageCommandBuilder WithQueueIdentifier(string value)
    {
        _queueIdentifier = value;
        return this;
    }

    public ResendFailedMessageCommandBuilder WithMessageId(string value)
    {
        _messageId = value;
        return this;
    }
    
    public ResendFailedMessageCommand Build()
        => new(_queueIdentifier, _messageId);
}