using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;

namespace SkyLegends.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders (My Orders)
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerEmail == userEmail || o.UserId == userEmail)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userEmail = User.Identity?.Name;
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id &&
                    (o.CustomerEmail == userEmail || o.UserId == userEmail));

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
