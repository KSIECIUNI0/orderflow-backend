using MediatR;
using OrderFlow.Application.Messaging;

namespace OrderFlow.Application.Dlq.Commands;

public record ProcessDlqMessageCommand(
    QueueMessage QueueMessage,
    DlqContext Context) : IRequest
{
}