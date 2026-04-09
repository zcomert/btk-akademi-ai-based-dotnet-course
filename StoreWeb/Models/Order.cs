using System.ComponentModel.DataAnnotations;

namespace StoreWeb.Models;

public class Order
{
    public int OrderId { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
