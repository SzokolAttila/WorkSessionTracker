using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    using WorkSessionTrackerAPI.ValidationAttributes;

    public class CreateWorkSessionDto
    {
        [Required] public DateTime StartDateTime { get; set; }
        [Required, EndDateGreaterThanStartDate(nameof(StartDateTime))] public DateTime EndDateTime { get; set; }
        public string? Description { get; set; }
    }
}
