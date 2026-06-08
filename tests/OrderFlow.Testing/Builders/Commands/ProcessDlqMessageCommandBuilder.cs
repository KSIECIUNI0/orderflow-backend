using OrderFlow.Application.Messaging;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Testing.Builders.DTOs;

namespace OrderFlow.Testing.Builders.Commands;

public class ProcessDlqMessageCommandBuilder
{
    private QueueMessage _queueMessage = new QueueMessageBuilder().Build();
    private string _eventType = "event type";
    private string _queueName = "queue name";

    public ProcessDlqMessageCommandBuilder WithQueueMessage(QueueMessage value)
    {
        _queueMessage = value;
        return this;
    }

    public ProcessDlqMessageCommandBuilder WithQueueName(string value)
    {
        _queueName = value;
        return this;
    }

    public ProcessDlqMessageCommandBuilder WithEventType(string value)
    {
        _eventType = value;
        return this;
    }
    
    public ProcessDlqMessageCommand Build()
    {
        return new ProcessDlqMessageCommand(_queueMessage, _eventType, _queueName);
    }
}