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
        [Required] public int EmployeeId { get; set; }
        public bool Verified { get; set; } = false;
        public string? Description { get; set; }

        [NotMapped] // This property will not be mapped to a database column
        public TimeSpan Duration
        {
            get => EndDateTime - StartDateTime;
        }
        [JsonIgnore]
        public Employee? Employee { get; set; } // Navigation property
    }
}
