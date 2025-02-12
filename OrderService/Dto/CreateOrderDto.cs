using OrderService.Models;

namespace OrderService.Dto;

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }
    public ICollection<OrderItemModel> OrderItems { get; set; }
}