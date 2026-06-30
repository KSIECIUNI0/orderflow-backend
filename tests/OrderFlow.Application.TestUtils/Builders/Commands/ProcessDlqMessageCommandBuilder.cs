using OrderFlow.Application.Dlq;
using OrderFlow.Application.Dlq.Commands;
using OrderFlow.Application.Messaging.DTOs;
using OrderFlow.Application.TestUtils.Builders.Dlq;
using OrderFlow.Application.TestUtils.Builders.DTOs;

namespace OrderFlow.Application.TestUtils.Builders.Commands;

public class ProcessDlqMessageCommandBuilder
{
    private QueueMessage _queueMessage = new QueueMessageBuilder().Build();
    private DlqContext _context = new DlqContextBuilder().Build();

    public ProcessDlqMessageCommandBuilder WithQueueMessage(QueueMessage value)
    {
        _queueMessage = value;
        return this;
    }

    public ProcessDlqMessageCommandBuilder WithDlqContext(DlqContext value)
    {
        _context = value;
        return this;
    }
    
    public ProcessDlqMessageCommand Build() => new(_queueMessage, _context);
}