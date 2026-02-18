using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyLegends.Data;
using SkyLegends.Models;
using Stripe;
using Stripe.Checkout;

namespace SkyLegends.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], webhookSecret);

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                    {
                        await HandleCheckoutCompleted(session);
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook error");
                return BadRequest();
            }
        }

        private async Task HandleCheckoutCompleted(Session session)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (order != null)
            {
                order.Status = "Paid";
                order.StripePaymentIntentId = session.PaymentIntentId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} marked as Paid via webhook", order.Id);
            }
        }
    }
}
