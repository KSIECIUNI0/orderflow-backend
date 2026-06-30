using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Failures.Commands;
using OrderFlow.Application.Failures.DTOs;
using OrderFlow.Application.Failures.Queries;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/failed-messages")]
public class FailedMessagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FailedMessagePage>> Get(
        [FromQuery] GetFailedMessagePageQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    [HttpPost("{queueIdentifier}/{messageId}/resend")]
    public async Task<IActionResult> Resend(
        [FromRoute] string queueIdentifier,
        [FromRoute] string messageId,
        CancellationToken cancellationToken)
    {
        var command = new ResendFailedMessageCommand(queueIdentifier, messageId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}