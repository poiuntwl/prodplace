namespace ProductsService.Dtos.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public IDictionary<string, string> CustomFields { get; set; } = new Dictionary<string, string>();
}