using WorkSessionTrackerAPI.Models;
using System.Text.Json.Serialization; // Add this using statement

namespace WorkSessionTrackerAPI.Models
{
    public class Student : User
    {
        public int? CompanyId { get; set; } // Nullable, a student might not have a company initially (Renamed from SupervisorId)

        [JsonIgnore] // Ignore this property during JSON serialization to break the cycle
        public Company? Company { get; set; } // Navigation property to the Company (Renamed from Supervisor)

        public ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();
    }
}
