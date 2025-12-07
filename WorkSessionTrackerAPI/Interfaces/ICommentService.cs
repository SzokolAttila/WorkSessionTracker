using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface ICommentService
    {
        Task<Comment?> CreateCommentAsync(CreateCommentDto dto, int supervisorId);
        Task<Comment?> UpdateCommentAsync(Comment existingComment, UpdateCommentDto dto);
        Task<bool> DeleteCommentAsync(Comment existingComment);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<Comment?> GetCommentByWorkSessionIdAsync(int workSessionId);
    }
}
