using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.RabbitMQRequests;

namespace ProductsService.Controllers;

[ApiController, Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IRabbitMqRpcClient _rabbitMqRpcClient;

    public ProductsController(AppDbContext dbContext, IRabbitMqRpcClient rabbitMqRpcClient)
    {
        _dbContext = dbContext;
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
    public async Task<IActionResult> UploadBulk()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var jsonContent = await reader.ReadToEndAsync();

            var records = JsonSerializer.Deserialize<ICollection<ProductDto>>(jsonContent);
            if (records == null || records.Count == 0)
            {
                return BadRequest("No valid records found in the JSON file.");
            }

            foreach (var r in records)
            {
                Console.WriteLine(JsonSerializer.Serialize(r));
            }

            // var result = await _recordService.ProcessRecordsAsync(records);

            // return Ok(new
            // { message = $"Processed {result.Created} new records and updated {result.Updated} existing records." });

            return Ok("Processed");
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
}