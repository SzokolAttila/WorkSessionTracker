using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.DTOs
{
    public class UpdateCommentDto
    {
        [Required] public string Content { get; set; } = string.Empty;
    }
}
