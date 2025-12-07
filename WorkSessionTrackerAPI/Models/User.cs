namespace WorkSessionTrackerAPI.Models
{
    public abstract class User // Make it abstract as you won't instantiate a 'User' directly
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Store password hashes, not plain text
        public string? EmailVerificationToken { get; set; } // Nullable, for email verification
        public DateTime? EmailVerificationTokenExpiration { get; set; } // When the token expires
        public bool EmailVerified { get; set; } = false; // Flag to indicate if email is verified
    }
}
