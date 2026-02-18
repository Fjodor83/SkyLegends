using System.ComponentModel.DataAnnotations;

namespace SkyLegends.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(40)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string DefaultStreetAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string DefaultStreetNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string DefaultCity { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string DefaultProvince { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string DefaultCountry { get; set; } = "Italia";

        public bool HasDefaultShippingAddress { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
