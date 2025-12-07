using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class CreateWorkSessionDto
    {
        [Required] public DateTime StartDateTime { get; set; }
        [Required] public DateTime EndDateTime { get; set; }
        public string? Description { get; set; }
    }
}
