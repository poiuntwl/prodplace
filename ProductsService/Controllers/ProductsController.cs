using Microsoft.AspNetCore.Mvc;
using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Mappers;

namespace ProductsService.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _dbContext.Products
            .Select(x => x.ToDto())
            .ToList();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _dbContext.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product.ToDto());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequestDto createDto)
    {
        var model = createDto.ToModel();
        _dbContext.Products.Add(model);
        _dbContext.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model.ToDto());
    }

    [HttpPut("id:int")]
    public IActionResult Update([FromQuery] int id, [FromBody] UpdateProductRequestDto dto)
    {
        var model = _dbContext.Products.Find(id);
        if (model == null)
        {
            return NotFound();
        }

        model.Name = dto.Name;
        model.Description = dto.Description;
        model.Price = dto.Price;

        _dbContext.SaveChanges();

        return Ok(model.ToDto());
    }
}