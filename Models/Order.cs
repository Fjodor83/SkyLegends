using System.ComponentModel.DataAnnotations;

namespace SkyLegends.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string StripeSessionId { get; set; } = string.Empty;

        public string? StripePaymentIntentId { get; set; }

        [Required]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ShippingAddress { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Paid, Shipped, Delivered

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<OrderItem> Items { get; set; } = new();
    }
}
