using System;
using System.ComponentModel.DataAnnotations;

namespace SkyLegends.Models
{
    public class Poster
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string Tags { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
