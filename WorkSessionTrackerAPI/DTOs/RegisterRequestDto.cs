using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        // This regular expression ensures the role is either "Student" or "Company", case-insensitive matching is handled in the controller.
        [RegularExpression("^(Student|Company)$", ErrorMessage = "Role must be either 'Student' or 'Company'.")]
        public string Role { get; set; } = string.Empty;
    }
}