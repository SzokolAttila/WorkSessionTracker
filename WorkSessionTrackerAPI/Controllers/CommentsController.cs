using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Linq;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All methods in this controller require authentication by default
    public class CommentsController : BaseApiController
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService; // Still needed for GetSupervisorWithEmployeesAsync
        private readonly UserManager<User> _userManager; // Inject UserManager for user-related checks
        private readonly IWorkSessionRepository _workSessionRepository; // Needed for work session existence and employeeId

        public CommentsController(ICommentService commentService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository) : base()
        {
            _commentService = commentService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Authorization: Only supervisors can create comments
            if (!User.IsInRole("Company"))
            {
                return Forbid("Only companies can create comments.");
            }

            // Fetch company with students for authorization (using IUserService)
            var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId); 
            if (company is null)
            {
                return Forbid("Authenticated user is not recognized as a company.");
            }

            // Fetch work session to check if it belongs to one of the supervisor's employees
            var workSession = await _workSessionRepository.GetByIdAsync(dto.WorkSessionId);
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            if (company.Students.All(s => s.Id != workSession.StudentId))
            {
                return Forbid("You can only comment on work sessions of your own students.");
            }

            var comment = await _commentService.CreateCommentAsync(dto, authenticatedUserId);
            if (comment == null)
            {
                return BadRequest("Could not create comment. A comment might already exist for this work session.");
            }
            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment is null) return NotFound("Comment not found.");

            // Authorization: Employee can view comments on their own work sessions, Supervisor can view comments on their employees' work sessions
            if (comment.WorkSession?.StudentId == authenticatedUserId) return Ok(comment); // Student owns the work session

            if (User.IsInRole("Company"))
            {
                var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId); // Use IUserService
                if (company != null && company.Students.Any(s => s.Id == comment.WorkSession?.StudentId))
                {
                    return Ok(comment); // Company of the student
                }
            }

            return Forbid("You are not authorized to view this comment.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null) return NotFound("Comment not found.");

            // Authorization: Only the supervisor who created the comment can update it
            if (!User.IsInRole("Company") || existingComment.CompanyId != authenticatedUserId)
            {
                return Forbid("You are not authorized to update this comment.");
            }

            var updatedComment = await _commentService.UpdateCommentAsync(existingComment, dto);
            if (updatedComment == null) return BadRequest("Failed to update comment.");
            return Ok(updatedComment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null) return NotFound("Comment not found.");

            // Authorization: Only the supervisor who created the comment can delete it
            if (!User.IsInRole("Company") || existingComment.CompanyId != authenticatedUserId)
            {
                return Forbid("You are not authorized to delete this comment.");
            }

            var result = await _commentService.DeleteCommentAsync(existingComment);
            if (!result) return BadRequest("Failed to delete comment.");
            return NoContent();
        }
    }
}
