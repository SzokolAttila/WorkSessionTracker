using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly IUserService _userService; // Needed for supervisor authorization checks
        private readonly IWorkSessionRepository _workSessionRepository; // Needed for work session existence and employeeId

        public CommentsController(ICommentService commentService, IUserService userService, IWorkSessionRepository workSessionRepository) : base()
        {
            _commentService = commentService;
            _userService = userService;
            _workSessionRepository = workSessionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Authorization: Only supervisors can create comments
            if (authenticatedUserRole != "Supervisor")
            {
                return Forbid("Only supervisors can create comments.");
            }

            // Fetch supervisor with employees for authorization
            var supervisor = await _userService.GetSupervisorWithEmployeesAsync(authenticatedUserId);
            if (supervisor == null)
            {
                return Forbid("Authenticated user is not recognized as a supervisor.");
            }

            // Fetch work session to check if it belongs to one of the supervisor's employees
            var workSession = await _workSessionRepository.GetByIdAsync(dto.WorkSessionId);
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            if (supervisor.Employees.All(e => e.Id != workSession.EmployeeId))
            {
                return Forbid("You can only comment on work sessions of your own employees.");
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
            var authenticatedUserRole = GetAuthenticatedUserRole();

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound("Comment not found.");

            // Authorization: Employee can view comments on their own work sessions, Supervisor can view comments on their employees' work sessions
            if (comment.WorkSession?.EmployeeId == authenticatedUserId) return Ok(comment); // Employee owns the work session

            if (authenticatedUserRole == "Supervisor")
            {
                var supervisor = await _userService.GetSupervisorWithEmployeesAsync(authenticatedUserId);
                if (supervisor != null && supervisor.Employees.Any(e => e.Id == comment.WorkSession?.EmployeeId))
                {
                    return Ok(comment); // Supervisor of the employee
                }
            }

            return Forbid("You are not authorized to view this comment.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var authenticatedUserRole = GetAuthenticatedUserRole();

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment == null) return NotFound("Comment not found.");

            // Authorization: Only the supervisor who created the comment can update it
            if (authenticatedUserRole != "Supervisor" || existingComment.SupervisorId != authenticatedUserId)
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
            var authenticatedUserRole = GetAuthenticatedUserRole();

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment == null) return NotFound("Comment not found.");

            // Authorization: Only the supervisor who created the comment can delete it
            if (authenticatedUserRole != "Supervisor" || existingComment.SupervisorId != authenticatedUserId)
            {
                return Forbid("You are not authorized to delete this comment.");
            }

            var result = await _commentService.DeleteCommentAsync(existingComment);
            if (!result) return BadRequest("Failed to delete comment.");
            return NoContent();
        }
    }
}
