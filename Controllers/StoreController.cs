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
            var posters = await _context.Posters
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posters);
        }

        // GET: Store/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poster = await _context.Posters
                .AsNoTracking()
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
            var videos = await _context.Videos
                .AsNoTracking()
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return View(videos);
        }
    }
}
