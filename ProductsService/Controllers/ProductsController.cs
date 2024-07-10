using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.RabbitMQRequests;

namespace ProductsService.Controllers;

[ApiController, Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IRabbitMqRpcClient _rabbitMqRpcClient;

    public ProductsController(IRabbitMqRpcClient rabbitMqRpcClient)
    {
        _rabbitMqRpcClient = rabbitMqRpcClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products =
            await _rabbitMqRpcClient.CallAsync<GetProductsQueueRequest, ICollection<ProductDto>>(
                new GetProductsQueueRequest(), ct);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product =
            await _rabbitMqRpcClient.CallAsync<GetProductQueueRequest, ProductDto>(new GetProductQueueRequest
                { Id = id }, ct);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto createDto, CancellationToken ct)
    {
        var createdId = await _rabbitMqRpcClient.CallAsync<CreateProductQueueRequest, int>(
            new CreateProductQueueRequest(createDto), ct);
        return CreatedAtAction(nameof(GetById), new { id = createdId });
    }

    [HttpPut("id:int")]
    public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateProductRequestDto dto,
        CancellationToken ct)
    {
        var model =
            await _rabbitMqRpcClient.CallAsync<GetProductsQueueRequest, ICollection<ProductDto>>(
                new GetProductsQueueRequest(), ct);
        if (model == null)
        {
            return NotFound();
        }

        await _rabbitMqRpcClient.CallAsync(new UpdateProductQueueRequest(dto), ct);

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
                await _rabbitMqRpcClient.CallAsync<BulkCreateUpdateQueueRequest, BulkCreateUpdateQueueResponse>(
                    new BulkCreateUpdateQueueRequest(records), ct);

            return Ok(new
            {
                created = result?.Created ?? 0,
                updated = result?.Updated ?? 0
            });
        }
        catch (JsonException ex)
        {
            return BadRequest($"Invalid JSON format: {ex.Message}");
        }
        catch (Exception)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while processing the file.");
        }
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test([FromServices] MongoDbContext ctx, CancellationToken ct)
    {
        var r = await ctx.Products.Find(x => x.Id == 1).FirstOrDefaultAsync(cancellationToken: ct);
        Console.WriteLine(JsonSerializer.Serialize(r));
        /*
        var r = await ctx.Products.AsQueryable().ToListAsync();
        foreach (var i in r)
        {
            Console.WriteLine(JsonSerializer.Serialize(i));
        }
        */

        return Ok();
    }
}