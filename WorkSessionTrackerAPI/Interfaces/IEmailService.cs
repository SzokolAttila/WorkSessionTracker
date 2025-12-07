using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}