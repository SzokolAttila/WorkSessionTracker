using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class VerifyEmailDto
    {
        [Required] public int UserId { get; set; }
        [Required] public string Token { get; set; } = string.Empty;
    }
}
