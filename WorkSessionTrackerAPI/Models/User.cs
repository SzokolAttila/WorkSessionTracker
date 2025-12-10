using Microsoft.AspNetCore.Identity;

namespace WorkSessionTrackerAPI.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty; // Keep if it's a display name, distinct from UserName
    }
}
