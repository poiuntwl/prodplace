using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Models.DatabaseModels;

public class PurchaseModel
{
    [Key] public int Id { get; set; }
    [ForeignKey("Product")] public int ProductId { get; set; }
    [Required] public ProductModel Product { get; set; }
    [ForeignKey("Customer")] public int CustomerId { get; set; }
    [Required] public CustomerModel Customer { get; set; }
}