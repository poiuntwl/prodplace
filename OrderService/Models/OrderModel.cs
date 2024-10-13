using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models;

public sealed class OrderModel
{
    [Key] public long Id { get; set; }

    [Required] public int CustomerId { get; set; }

    [Required] public DateTimeOffset OrderDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; }

    [Required] [MaxLength(20)] public OrderStatus Status { get; set; }

    [Required] public string ShippingAddress { get; set; }

    [Required] public string BillingAddress { get; set; }

    public ICollection<OrderItemModel> OrderItems { get; set; }
}