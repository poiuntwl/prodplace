namespace ProductsService.Dtos.Product;

public class UpdateProductRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string CustomFields { get; set; }
}