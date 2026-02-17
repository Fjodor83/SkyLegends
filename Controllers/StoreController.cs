using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;
using SkyLegends.Models;

namespace SkyLegends.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Store
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posters.ToListAsync());
        }

        // GET: Store/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poster = await _context.Posters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (poster == null)
            {
                return NotFound();
            }

            return View(poster);
        }

        // GET: Store/Videos
        public async Task<IActionResult> Videos()
        {
            return View(await _context.Videos.ToListAsync());
        }
    }
}
