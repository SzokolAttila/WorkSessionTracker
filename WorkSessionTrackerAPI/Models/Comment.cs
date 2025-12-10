using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkSessionTrackerAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required] public int WorkSessionId { get; set; }
        [Required] public int CompanyId { get; set; } // Renamed from SupervisorId
        [Required] public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore] // Prevent serialization cycle
        public WorkSession? WorkSession { get; set; } // Navigation property to the WorkSession
        [JsonIgnore] // Prevent serialization cycle
        public Company? Company { get; set; } // Navigation property to the Company who made the comment (Renamed from Supervisor)
    }
}
