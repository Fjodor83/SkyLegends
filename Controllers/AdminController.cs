using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;
using SkyLegends.Models;

namespace SkyLegends.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Posters
        public async Task<IActionResult> Posters()
        {
            return View(await _context.Posters.ToListAsync());
        }

        // GET: Admin/CreatePoster
        public IActionResult CreatePoster()
        {
            return View();
        }

        // POST: Admin/CreatePoster
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePoster([Bind("Title,Description,Tags")] Poster poster, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "img", "posters");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    poster.ImageUrl = "/img/posters/" + uniqueFileName;
                }
                else
                {
                    poster.ImageUrl = "/img/posters/placeholder.jpg"; // Default or error
                }

                poster.CreatedAt = DateTime.UtcNow;
                _context.Add(poster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Posters));
            }
            return View(poster);
        }

        // GET: Admin/DeletePoster/5
        public async Task<IActionResult> DeletePoster(int? id)
        {
            if (id == null) return NotFound();

            var poster = await _context.Posters.FirstOrDefaultAsync(m => m.Id == id);
            if (poster == null) return NotFound();

            return View(poster);
        }

        // POST: Admin/DeletePoster/5
        [HttpPost, ActionName("DeletePoster")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePosterConfirmed(int id)
        {
            var poster = await _context.Posters.FindAsync(id);
            if (poster != null)
            {
                _context.Posters.Remove(poster);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Posters));
        }
        // GET: Admin/EditPoster/5
        public async Task<IActionResult> EditPoster(int? id)
        {
            if (id == null) return NotFound();

            var poster = await _context.Posters.FindAsync(id);
            if (poster == null) return NotFound();
            return View(poster);
        }

        // POST: Admin/EditPoster/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPoster(int id, [Bind("Id,Title,Description,ImageUrl,Tags,CreatedAt")] Poster poster, IFormFile? imageFile)
        {
            if (id != poster.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "img", "posters");

                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        poster.ImageUrl = "/img/posters/" + uniqueFileName;
                    }

                    _context.Update(poster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Posters.Any(e => e.Id == poster.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Posters));
            }
            return View(poster);
        }

        // GET: Admin/Videos
        public async Task<IActionResult> Videos()
        {
            return View(await _context.Videos.ToListAsync());
        }

        // GET: Admin/CreateVideo
        public IActionResult CreateVideo()
        {
            return View();
        }

        // POST: Admin/CreateVideo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideo([Bind("Title,Description,VideoUrl,ThumbnailUrl")] Video video)
        {
            if (ModelState.IsValid)
            {
                video.CreatedAt = DateTime.UtcNow;
                _context.Add(video);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Videos));
            }
            return View(video);
        }

        // GET: Admin/DeleteVideo/5
        public async Task<IActionResult> DeleteVideo(int? id)
        {
            if (id == null) return NotFound();

            var video = await _context.Videos.FirstOrDefaultAsync(m => m.Id == id);
            if (video == null) return NotFound();

            return View(video);
        }

        // POST: Admin/DeleteVideo/5
        [HttpPost, ActionName("DeleteVideo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideoConfirmed(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Videos));
        }
    }
}
