using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Handlers;
using ProductsService.Mappers;

namespace ProductsService.Controllers;

[ApiController, Route("/api")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await _mediator.Send(new GetProductsRequest(), ct);
        return Ok(products);
    }

    [HttpGet("{id=id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var product = await _mediator.Send(new GetProductRequest(ObjectId.Parse(id)), ct);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto createDto, CancellationToken ct)
    {
        try
        {
            var createdId = await _mediator.Send(new CreateProductRequest(createDto), ct);
            return await GetById(createdId.ToString(), ct);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequestDto dto,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductRequest(id, dto), ct);
        return Ok();
    }

    [HttpPost("upload"), Consumes("application/json")]
    public async Task<IActionResult> UploadBulk(CancellationToken ct)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var jsonContent = await reader.ReadToEndAsync(ct);

            var records = JsonSerializer.Deserialize<ICollection<ProductDto>>(jsonContent);
            if (records == null || records.Count == 0)
            {
                return BadRequest("No valid records found in the JSON file.");
            }

            var result =
                await _mediator.Send(new CreateOrUpdateProductsRequest(records.Select(x => x.ToModel()).ToList()), ct);

            return Ok(new
            {
                created = result.Created,
                updated = result.Updated
            });
        }
        catch (JsonException ex)
        {
            return BadRequest($"Invalid JSON format: {ex.Message}");
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the file.");
        }
    }
}