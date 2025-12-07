using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IWorkSessionRepository _workSessionRepository; // To check if work session exists

        public CommentService(ICommentRepository commentRepository, IWorkSessionRepository workSessionRepository)
        {
            _commentRepository = commentRepository;
            _workSessionRepository = workSessionRepository;
        }

        public async Task<Comment?> CreateCommentAsync(CreateCommentDto dto, int supervisorId)
        {
            // Check if work session exists (further authorization is in controller)
            var workSession = await _workSessionRepository.GetByIdAsync(dto.WorkSessionId);
            if (workSession == null) return null;

            // Check if a comment already exists for this work session
            if (await _commentRepository.GetCommentByWorkSessionIdAsync(dto.WorkSessionId) != null)
            {
                return null; // Work session already has a comment
            }

            var comment = new Comment
            {
                WorkSessionId = dto.WorkSessionId,
                SupervisorId = supervisorId,
                Content = dto.Content
            };

            await _commentRepository.AddAsync(comment);
            return comment;
        }

        public async Task<Comment?> UpdateCommentAsync(Comment existingComment, UpdateCommentDto dto)
        {
            existingComment.Content = dto.Content;
            await _commentRepository.UpdateAsync(existingComment);
            return existingComment;
        }

        public async Task<bool> DeleteCommentAsync(Comment existingComment)
        {
            await _commentRepository.DeleteAsync(existingComment);
            return true;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId) => await _commentRepository.GetByIdAsync(commentId);
        public async Task<Comment?> GetCommentByWorkSessionIdAsync(int workSessionId) => await _commentRepository.GetCommentByWorkSessionIdAsync(workSessionId);
    }
}
