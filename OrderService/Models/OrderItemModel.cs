using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models;

public sealed class OrderItemModel
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long OrderId { get; set; }

    [Required]
    [MaxLength(24)] // handle mongodb id. Introduces coupling.
    public string ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    [ForeignKey("OrderId")]
    public OrderModel Order { get; set; }
}