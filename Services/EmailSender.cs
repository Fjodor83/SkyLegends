using Microsoft.AspNetCore.Identity.UI.Services;

namespace SkyLegends.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, we just log or do nothing.
            // In a real app, integrate an email provider here.
            return Task.CompletedTask;
        }
    }
}
