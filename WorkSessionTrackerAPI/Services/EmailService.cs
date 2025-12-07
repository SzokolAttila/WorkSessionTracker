using WorkSessionTrackerAPI.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics; // For Debug.WriteLine

namespace WorkSessionTrackerAPI.Services
{
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            Debug.WriteLine($"Sending email to: {toEmail}, Subject: {subject}, Message: {message}");
            return Task.CompletedTask; // In a real app, this would integrate with an email provider
        }
    }
}
