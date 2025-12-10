using System.Text.Json.Serialization;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Models
{
    public class Company : User
    {
        public ICollection<Student> Students { get; set; } = new List<Student>(); // Renamed from Employees
        [JsonIgnore] // Ignore this property during JSON serialization to break the cycle
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
