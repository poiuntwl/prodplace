namespace ProductsService.Dtos.Product;

public class CreateProductRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CustomFields { get; set; } = string.Empty;
}