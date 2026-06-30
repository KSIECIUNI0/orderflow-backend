using MediatR;
using OrderFlow.Application.Messaging.DTOs;

namespace OrderFlow.Application.Dlq.Commands;

public record ProcessDlqMessageCommand(
    QueueMessage QueueMessage,
    DlqContext Context) : IRequest
{
}