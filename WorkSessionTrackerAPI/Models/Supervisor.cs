using System.Text.Json.Serialization;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Models
{
    public class Supervisor : User
    {
        public string TotpSeed { get; set; } = string.Empty; // For Two-Factor Authentication

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        [JsonIgnore] // Ignore this property during JSON serialization to break the cycle
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
