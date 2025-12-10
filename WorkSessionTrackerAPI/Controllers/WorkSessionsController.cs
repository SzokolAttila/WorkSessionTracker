using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All methods in this controller require authentication by default
    public class WorkSessionsController : BaseApiController // Inherit from BaseApiController
    {
        private readonly IWorkSessionService _workSessionService;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager; // Inject UserManager for user-related checks

        private readonly IWorkSessionRepository _workSessionRepository; // Added for direct GetByIdAsync calls
        public WorkSessionsController(IWorkSessionService workSessionService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository) : base() // Call base constructor
        {
            _workSessionService = workSessionService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkSession([FromBody] CreateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Authorization: Only an Employee can create a work session for themselves
            if (!User.IsInRole("Student"))
            {
                return Forbid("Only students can create work sessions.");
            }

            // Ensure the authenticated user is actually a student (redundant if role check is strict, but good for safety)
            var user = await _userManager.FindByIdAsync(authenticatedUserId.ToString());
            if (user is not Student student) // Check if the user is a Student
            {
                return Forbid("Authenticated user is not recognized as a student.");
            }

            var workSession = await _workSessionService.CreateWorkSessionAsync(dto, authenticatedUserId);
            if (workSession == null)
            {
                return BadRequest("Could not create work session. Ensure employee exists.");
            }
            return Ok(workSession); // Changed from CreatedAtAction to Ok
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetWorkSessionsForStudent(int studentId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Authorization logic moved to controller
            if (studentId == authenticatedUserId)
            {
                // Student viewing their own work sessions
                var workSessions = await _workSessionService.GetStudentWorkSessionsAsync(studentId);
                return Ok(workSessions);
            }
            else if (User.IsInRole("Company"))
            {
                // Company viewing their student's work sessions
                var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);
                if (company != null && company.Students.Any(s => s.Id == studentId))
                {
                    var workSessions = await _workSessionService.GetStudentWorkSessionsAsync(studentId);
                    return Ok(workSessions);
                }
            }

            // If none of the above, then not authorized
            return Forbid("You are not authorized to view these work sessions.");
        }

        [HttpPut("{id}")] // Get ID from route
        public async Task<IActionResult> UpdateWorkSession(int id, [FromBody] UpdateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the employee who owns the work session can update it
            if (!User.IsInRole("Student") || workSession.StudentId != authenticatedUserId)
            {
                return Forbid("You are not authorized to update this work session.");
            }

            var updatedWorkSession = await _workSessionService.UpdateWorkSessionAsync(workSession, dto);
            if (updatedWorkSession == null)
            {
                return BadRequest("Failed to update work session.");
            }
            return Ok(updatedWorkSession);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkSession(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the employee who owns the work session can delete it
            if (!User.IsInRole("Student") || workSession.StudentId != authenticatedUserId)
            {
                return Forbid("You are not authorized to delete this work session.");
            }

            var result = await _workSessionService.DeleteWorkSessionAsync(workSession);
            if (!result)
            {
                return BadRequest("Failed to delete work session.");
            }
            return NoContent();
        }

        [HttpPost("verify/{id}")] // Changed to POST, ID from route
        public async Task<IActionResult> VerifyStudentWorkSession(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            // Fetch the work session to check supervisor relationship
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the supervisor of the employee can verify the work session
            if (!User.IsInRole("Company"))
            {
                return Forbid("Only companies can verify work sessions.");
            }

            var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId); // Use IUserService for this custom logic
            if (company is null || company.Students.All(s => s.Id != workSession.StudentId))
            {
                return Forbid("You are not authorized to verify this student's work session.");
            }

            var verifiedWorkSession = await _workSessionService.VerifyWorkSessionAsync(workSession);
            if (verifiedWorkSession == null)
            {
                return BadRequest("Failed to verify work session.");
            }
            return Ok(verifiedWorkSession);
        }
    }
}