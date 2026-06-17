using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllOrdersQuery();
        var result = await mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(request.ProductName, request.Quantity);
        var result = await mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Id },
            result
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateOrder(
        Guid id,
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderCommand(id, request.ProductName, request.Quantity);
        var success = await mediator.Send(command, cancellationToken);
        
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(("{id:guid}"))]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        
        return success ? NoContent() : NotFound();
    }
}