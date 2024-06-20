using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsService.Models;

public class ProductModel
{
    public int Id { get; set; }
    [Column(TypeName = "nvarchar(20)")] public string Name { get; set; } = string.Empty;
    [Column(TypeName = "nvarchar(max)")] public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    [Column(TypeName = "nvarchar(max)")] public string CustomFields { get; set; } = string.Empty;
}