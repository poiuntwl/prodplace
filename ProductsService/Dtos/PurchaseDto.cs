namespace ProductsService.Models;

public class PurchaseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public ProductModel Product { get; set; }
    public int CustomerId { get; set; }
    public CustomerModel Customer { get; set; }
}