using WorkSessionTrackerAPI.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics; // For Debug.WriteLine
using Microsoft.Extensions.Logging;

namespace WorkSessionTrackerAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogInformation("Sending email to: {ToEmail}, Subject: {Subject}, Message: {Message}", toEmail, subject, message);
            return Task.CompletedTask; // In a real app, this would integrate with an email provider
        }
    }
}
