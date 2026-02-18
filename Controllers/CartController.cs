using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;
using SkyLegends.Models;
using SkyLegends.Services;

namespace SkyLegends.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.Total = _cartService.GetTotal();
            return View(cart);
        }

        // POST: Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int posterId, int quantity = 1)
        {
            var poster = await _context.Posters.FindAsync(posterId);
            if (poster == null || !poster.IsAvailable)
            {
                return NotFound();
            }

            _cartService.AddToCart(new CartItem
            {
                PosterId = poster.Id,
                Title = poster.Title,
                ImageUrl = poster.ImageUrl,
                Price = poster.Price,
                Quantity = quantity
            });

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int posterId)
        {
            _cartService.RemoveFromCart(posterId);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int posterId, int quantity)
        {
            _cartService.UpdateQuantity(posterId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Count (AJAX endpoint for navbar badge)
        [HttpGet]
        public IActionResult Count()
        {
            return Json(new { count = _cartService.GetItemCount() });
        }
    }
}
