using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class CreateCommentDto
    {
        [Required] public int WorkSessionId { get; set; }
        [Required] public string Content { get; set; } = string.Empty;
    }
}
