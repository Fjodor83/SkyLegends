using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;
using SkyLegends.Models;

namespace SkyLegends.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            var model = profile == null
                ? new ProfileViewModel
                {
                    Email = user.Email ?? User.Identity?.Name ?? string.Empty,
                    Country = "Italia"
                }
                : new ProfileViewModel
                {
                    FullName = profile.FullName,
                    Email = profile.Email,
                    PhoneNumber = profile.PhoneNumber,
                    StreetAddress = profile.DefaultStreetAddress,
                    StreetNumber = profile.DefaultStreetNumber,
                    City = profile.DefaultCity,
                    Province = profile.DefaultProvince,
                    Country = profile.DefaultCountry
                };

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id
                };
                _context.UserProfiles.Add(profile);
            }

            profile.FullName = model.FullName.Trim();
            profile.Email = model.Email.Trim();
            profile.PhoneNumber = model.PhoneNumber.Trim();
            profile.DefaultStreetAddress = model.StreetAddress.Trim();
            profile.DefaultStreetNumber = model.StreetNumber.Trim();
            profile.DefaultCity = model.City.Trim();
            profile.DefaultProvince = model.Province.Trim();
            profile.DefaultCountry = model.Country.Trim();
            profile.HasDefaultShippingAddress = true;
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["ProfileSaved"] = "Profilo aggiornato con successo.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
