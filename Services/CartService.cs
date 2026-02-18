using System.Text.Json;
using SkyLegends.Models;

namespace SkyLegends.Services
{
    public class CartService
    {
        private const string CartSessionKey = "SkyLegendsCart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetCart()
        {
            var json = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            Session.SetString(CartSessionKey, json);
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.PosterId == item.PosterId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }

            SaveCart(cart);
        }

        public void RemoveFromCart(int posterId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.PosterId == posterId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int posterId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.PosterId == posterId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        public int GetItemCount()
        {
            return GetCart().Sum(c => c.Quantity);
        }

        public decimal GetTotal()
        {
            return GetCart().Sum(c => c.Total);
        }
    }
}
