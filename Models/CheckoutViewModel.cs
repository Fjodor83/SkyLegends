using System.ComponentModel.DataAnnotations;

namespace SkyLegends.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();

        [Required]
        [Display(Name = "Nome e cognome")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Numero di telefono")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Indirizzo")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Numero civico")]
        public string StreetNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Localita")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Provincia")]
        public string Province { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Paese")]
        public string Country { get; set; } = "Italia";

        public string ShippingAddress =>
            $"{StreetAddress}, {StreetNumber}, {City}, {Province}, {Country}";
    }
}
