using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Models
{
    public class Supervisor : User
    {
        public string TotpSeed { get; set; } = string.Empty; // For Two-Factor Authentication

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
