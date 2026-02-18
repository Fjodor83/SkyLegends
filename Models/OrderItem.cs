using System.ComponentModel.DataAnnotations;

namespace SkyLegends.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int PosterId { get; set; }
        public Poster Poster { get; set; } = null!;

        public string PosterTitle { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }
}
