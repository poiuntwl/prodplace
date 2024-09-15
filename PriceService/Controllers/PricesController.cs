using MediatR;
using Microsoft.AspNetCore.Mvc;
using PriceService.Commands;

namespace PriceService.Controllers;

[ApiController, Route("/api")]
public class PricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{productId:int}/{priceAmount:decimal}")]
    public async Task<IActionResult> Update(int productId, decimal priceAmount, CancellationToken ct)
    {
        var query = new UpdatePriceCommand(productId, priceAmount);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}