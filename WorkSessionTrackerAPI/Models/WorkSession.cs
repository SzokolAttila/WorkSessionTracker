using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkSessionTrackerAPI.Models
{
    public class WorkSession
    {
        public int Id { get; set; }
        [Required] public DateTime StartDateTime { get; set; }
        [Required] public DateTime EndDateTime { get; set; }
        [Required] public int EmployeeId { get; set; }
        public bool Verified { get; set; } = false;
        public string? Description { get; set; }
        [JsonIgnore]
        public Employee? Employee { get; set; } // Navigation property
    }
}
