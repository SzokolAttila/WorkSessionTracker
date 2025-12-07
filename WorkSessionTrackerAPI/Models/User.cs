namespace WorkSessionTrackerAPI.Models
{
    public abstract class User // Make it abstract as you won't instantiate a 'User' directly
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Store password hashes, not plain text
        public string? EmailToken { get; set; } // Nullable, as it might not always be present or needed
    }
}
