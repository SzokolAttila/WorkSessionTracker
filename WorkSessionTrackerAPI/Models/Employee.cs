using WorkSessionTrackerAPI.Models;
using System.Text.Json.Serialization; // Add this using statement

namespace WorkSessionTrackerAPI.Models
{
    public class Employee : User
    {
        public int? SupervisorId { get; set; } // Nullable, an employee might not have a supervisor initially

        [JsonIgnore] // Ignore this property during JSON serialization to break the cycle
        public Supervisor? Supervisor { get; set; } // Navigation property to the Supervisor

        public ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();
    }
}
