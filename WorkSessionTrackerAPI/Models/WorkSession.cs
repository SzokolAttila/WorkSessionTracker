using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using System.ComponentModel.DataAnnotations.Schema; // Add this using statement for [NotMapped]
namespace WorkSessionTrackerAPI.Models
{
    using WorkSessionTrackerAPI.ValidationAttributes;

    public class WorkSession
    {
        public int Id { get; set; }
        [Required] public DateTime StartDateTime { get; set; }
        [Required, EndDateGreaterThanStartDate(nameof(StartDateTime))] public DateTime EndDateTime { get; set; }
        [Required] public int StudentId { get; set; } // Renamed from EmployeeId
        public bool Verified { get; set; } = false;
        public string? Description { get; set; }

        [NotMapped] // This property will not be mapped to a database column
        public TimeSpan Duration
        {
            get => EndDateTime - StartDateTime;
        }
        [JsonIgnore]
        public Student? Student { get; set; } // Navigation property (Renamed from Employee)

        public Comment? Comment { get; set; } // One-to-one navigation property
    }
}
