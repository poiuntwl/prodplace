using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Models.DatabaseModels;

public class PurchaseModel
{
    [Key] public int Id { get; set; }
    [Column(TypeName = "nvarchar(50)")]
    public string ProductId { get; set; } = string.Empty;
    // Removed navigation property to ProductModel as it is a Mongo entity
    [ForeignKey("Customer")] public int CustomerId { get; set; }
    [Required] public CustomerModel Customer { get; set; }

    [Column(TypeName = "datetime2(2)")]
    [Required]
    public DateTime PurchaseTime { get; set; }
}