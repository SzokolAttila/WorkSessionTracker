using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Models
{
    public class Employee : User
    {
        public int? SupervisorId { get; set; } // Nullable, an employee might not have a supervisor initially

        public Supervisor? Supervisor { get; set; } // Navigation property to the Supervisor
    }
}
