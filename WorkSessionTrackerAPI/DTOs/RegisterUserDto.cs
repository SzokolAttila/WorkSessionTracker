using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class RegisterUserDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required]
        [RegularExpression(
            "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+=\\[{\\]};:<>|./?,-]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include at least one digit, one lowercase letter, one uppercase letter, and one special character."
        )]
        public string Password { get; set; } = string.Empty;
    }
}
