using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SkyLegends.Data;
using SkyLegends.Models;
using SkyLegends.Services;
using Stripe;
using Stripe.Checkout;

namespace SkyLegends.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            CartService cartService,
            ApplicationDbContext context,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager)
        {
            _cartService = cartService;
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            string? profileEmail = null;
            string? profileFullName = null;
            string? profilePhone = null;
            string? profileStreetAddress = null;
            string? profileStreetNumber = null;
            string? profileCity = null;
            string? profileProvince = null;
            string? profileCountry = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var profile = await _context.UserProfiles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    var hasDefaultAddress = profile is not null
                        && profile.HasDefaultShippingAddress
                        && !string.IsNullOrWhiteSpace(profile.DefaultStreetAddress)
                        && !string.IsNullOrWhiteSpace(profile.DefaultStreetNumber)
                        && !string.IsNullOrWhiteSpace(profile.DefaultCity)
                        && !string.IsNullOrWhiteSpace(profile.DefaultProvince)
                        && !string.IsNullOrWhiteSpace(profile.DefaultCountry);

                    if (!hasDefaultAddress)
                    {
                        TempData["ProfileWarning"] = "Devi impostare un indirizzo predefinito di consegna prima del pagamento.";
                        return RedirectToAction("Index", "Profile", new { returnUrl = Url.Action("Index", "Checkout") });
                    }

                    if (profile is not null)
                    {
                        profileEmail = profile.Email;
                        profileFullName = profile.FullName;
                        profilePhone = profile.PhoneNumber;
                        profileStreetAddress = profile.DefaultStreetAddress;
                        profileStreetNumber = profile.DefaultStreetNumber;
                        profileCity = profile.DefaultCity;
                        profileProvince = profile.DefaultProvince;
                        profileCountry = profile.DefaultCountry;
                    }
                }
            }

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                Email = profileEmail ?? (User.Identity?.IsAuthenticated == true ? User.Identity?.Name ?? string.Empty : string.Empty),
                CustomerName = profileFullName ?? string.Empty,
                PhoneNumber = profilePhone ?? string.Empty,
                StreetAddress = profileStreetAddress ?? string.Empty,
                StreetNumber = profileStreetNumber ?? string.Empty,
                City = profileCity ?? string.Empty,
                Province = profileProvince ?? string.Empty,
                Country = profileCountry ?? "Italia"
            };

            ViewBag.Total = _cartService.GetTotal();
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View(model);
        }

        // POST: Checkout/CreateSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
            {
                model.CartItems = cart;
                ViewBag.Total = _cartService.GetTotal();
                ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
                return View("Index", model);
            }

            // ── MOCK MODE ──────────────────────────────────────────────────────
            bool mockMode = _configuration.GetValue<bool>("Stripe:MockMode");
            if (mockMode)
            {
                if (User.Identity?.IsAuthenticated == true)
                    await UpsertDefaultAddressAsync(model);

                var mockSessionId = $"mock_{Guid.NewGuid():N}";
                var mockOrder = new Order
                {
                    StripeSessionId   = mockSessionId,
                    CustomerEmail     = model.Email,
                    CustomerName      = model.CustomerName,
                    PhoneNumber       = model.PhoneNumber,
                    ShippingAddress   = model.ShippingAddress,
                    TotalAmount       = _cartService.GetTotal(),
                    Status            = "Paid",
                    UserId            = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null,
                    Items = cart.Select(item => new OrderItem
                    {
                        PosterId     = item.PosterId,
                        PosterTitle  = item.Title,
                        Quantity     = item.Quantity,
                        UnitPrice    = item.Price
                    }).ToList()
                };
                _context.Orders.Add(mockOrder);
                await _context.SaveChangesAsync();
                _cartService.ClearCart();

                TempData["MockEmail"] = model.Email;
                return RedirectToAction(nameof(Success), new { mock_order_id = mockOrder.Id });
            }
            // ──────────────────────────────────────────────────────────────────

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            if (User.Identity?.IsAuthenticated == true)
                await UpsertDefaultAddressAsync(model);

            var domain = $"{Request.Scheme}://{Request.Host}";

            var lineItems = cart.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = item.Price * 100,
                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Title,
                        Images = new List<string> { $"{domain}{item.ImageUrl}" }
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Checkout/Cancel",
                CustomerEmail = model.Email,
                PhoneNumberCollection = new SessionPhoneNumberCollectionOptions { Enabled = true },
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "IT", "DE", "FR", "ES", "GB", "US", "AT", "CH", "BE", "NL" }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["customer_name"]  = model.CustomerName,
                    ["street_address"] = model.StreetAddress,
                    ["street_number"]  = model.StreetNumber,
                    ["city"]           = model.City,
                    ["province"]       = model.Province,
                    ["country"]        = model.Country,
                    ["phone_number"]   = model.PhoneNumber
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            var existingOrder = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (existingOrder == null)
            {
                var pendingOrder = new Order
                {
                    StripeSessionId = session.Id,
                    CustomerEmail   = model.Email,
                    CustomerName    = model.CustomerName,
                    PhoneNumber     = model.PhoneNumber,
                    ShippingAddress = model.ShippingAddress,
                    TotalAmount     = _cartService.GetTotal(),
                    Status          = "Pending",
                    UserId          = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null,
                    Items = cart.Select(item => new OrderItem
                    {
                        PosterId    = item.PosterId,
                        PosterTitle = item.Title,
                        Quantity    = item.Quantity,
                        UnitPrice   = item.Price
                    }).ToList()
                };
                _context.Orders.Add(pendingOrder);
                await _context.SaveChangesAsync();
            }

            return Redirect(session.Url);
        }

        // GET: Checkout/Success
        public async Task<IActionResult> Success(string? session_id, int? mock_order_id)
        {
            // ── MOCK MODE ──────────────────────────────────────────────────────
            if (mock_order_id.HasValue)
            {
                var mockOrder = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == mock_order_id.Value);

                ViewBag.IsMock        = true;
                ViewBag.SessionId     = mockOrder?.StripeSessionId;
                ViewBag.CustomerEmail = TempData["MockEmail"] ?? mockOrder?.CustomerEmail;
                return View();
            }
            // ──────────────────────────────────────────────────────────────────

            if (string.IsNullOrEmpty(session_id))
                return RedirectToAction("Index", "Home");

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var service = new SessionService();
            var session = service.Get(session_id);

            // Check if order already exists (idempotency)
            var existingOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripeSessionId == session_id);
            if (existingOrder == null)
            {
                var cart = _cartService.GetCart();

                var order = new Order
                {
                    StripeSessionId = session_id,
                    StripePaymentIntentId = session.PaymentIntentId,
                    CustomerEmail = session.CustomerEmail ?? session.CustomerDetails?.Email ?? "",
                    CustomerName = session.CustomerDetails?.Name,
                    PhoneNumber = session.CustomerDetails?.Phone,
                    ShippingAddress = BuildShippingAddress(session.CustomerDetails?.Address),
                    TotalAmount = (session.AmountTotal ?? 0) / 100m, // Convert from cents
                    Status = session.PaymentStatus == "paid" ? "Paid" : "Pending",
                    UserId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null,
                    Items = cart.Select(item => new OrderItem
                    {
                        PosterId = item.PosterId,
                        PosterTitle = item.Title,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
            }
            else
            {
                existingOrder.StripePaymentIntentId = session.PaymentIntentId;
                existingOrder.Status = session.PaymentStatus == "paid" ? "Paid" : existingOrder.Status;
                if (!string.IsNullOrWhiteSpace(session.CustomerEmail))
                {
                    existingOrder.CustomerEmail = session.CustomerEmail;
                }
                if (!string.IsNullOrWhiteSpace(session.CustomerDetails?.Name))
                {
                    existingOrder.CustomerName = session.CustomerDetails.Name;
                }
                if (!string.IsNullOrWhiteSpace(session.CustomerDetails?.Phone))
                {
                    existingOrder.PhoneNumber = session.CustomerDetails.Phone;
                }
                var shippingAddress = BuildShippingAddress(session.CustomerDetails?.Address);
                if (!string.IsNullOrWhiteSpace(shippingAddress))
                {
                    existingOrder.ShippingAddress = shippingAddress;
                }
                if (session.AmountTotal.HasValue)
                {
                    existingOrder.TotalAmount = session.AmountTotal.Value / 100m;
                }
            }

            await _context.SaveChangesAsync();
            _cartService.ClearCart();

            ViewBag.SessionId = session_id;
            ViewBag.CustomerEmail = session.CustomerEmail ?? session.CustomerDetails?.Email;
            return View();
        }

        // GET: Checkout/Cancel
        public IActionResult Cancel()
        {
            return View();
        }

        private static string? BuildShippingAddress(Address? address)
        {
            if (address == null)
            {
                return null;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(address.Line1)) parts.Add(address.Line1);
            if (!string.IsNullOrWhiteSpace(address.Line2)) parts.Add(address.Line2);
            if (!string.IsNullOrWhiteSpace(address.City)) parts.Add(address.City);
            if (!string.IsNullOrWhiteSpace(address.State)) parts.Add(address.State);
            if (!string.IsNullOrWhiteSpace(address.PostalCode)) parts.Add(address.PostalCode);
            if (!string.IsNullOrWhiteSpace(address.Country)) parts.Add(address.Country);

            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        private async Task UpsertDefaultAddressAsync(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = _context.UserProfiles.Add(new()).Entity;
                profile.UserId = user.Id;
            }

            profile.FullName = model.CustomerName.Trim();
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
        }
    }
}
