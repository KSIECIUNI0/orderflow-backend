using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Return list of all orders.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Create  new order.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.ProductName, request.Quantity);
        var result = await _mediator.Send(command);
        
        return CreatedAtAction(
            nameof(GetAll), // TO DO: Change to endpoint - get by id
            new { id = result.Id },
            result
        );
    }
}