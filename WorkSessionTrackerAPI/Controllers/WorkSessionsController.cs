using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Authorization;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using Microsoft.Extensions.Logging;

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
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<WorkSessionsController> _logger;

        private readonly IWorkSessionRepository _workSessionRepository; // Added for direct GetByIdAsync calls
        public WorkSessionsController(IWorkSessionService workSessionService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository, IAuthorizationService authorizationService, ILogger<WorkSessionsController> logger) : base() // Call base constructor
        {
            _workSessionService = workSessionService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = Policies.StudentOnly)]
        public async Task<IActionResult> CreateWorkSession([FromBody] CreateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} is creating a new work session.", authenticatedUserId);

            var workSession = await _workSessionService.CreateWorkSessionAsync(dto, authenticatedUserId);
            if (workSession == null)
            {
                _logger.LogWarning("Failed to create work session for user {UserId}. The service returned null.", authenticatedUserId);
                return BadRequest("Could not create work session. Ensure employee exists.");
            }

            _logger.LogInformation("Successfully created WorkSession {WorkSessionId} for User {UserId}", workSession.Id, authenticatedUserId);
            return Ok(workSession); // Changed from CreatedAtAction to Ok
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetWorkSessionsForStudent(int studentId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to get work sessions for student {StudentId}.", authenticatedUserId, studentId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, studentId, Policies.CanAccessStudentData);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to view work sessions for student {StudentId}.", authenticatedUserId, studentId);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to view these work sessions.");
            }

            var workSessions = await _workSessionService.GetStudentWorkSessionsAsync(studentId);
            _logger.LogInformation("User {UserId} successfully retrieved work sessions for student {StudentId}.", authenticatedUserId, studentId);
            return Ok(workSessions);
        }

        [HttpGet("student/{studentId}/summary")]
        public async Task<IActionResult> GetWorkSessionSummary(int studentId, [FromQuery] int year, [FromQuery] int month)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation(
                "User {UserId} attempting to get work session summary for student {StudentId} for {Year}-{Month}.",
                authenticatedUserId, studentId, year, month);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, studentId, Policies.CanAccessStudentData);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning(
                    "User {UserId} is not authorized to view work session summary for student {StudentId}.",
                    authenticatedUserId, studentId);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to view this data.");
            }

            if (year < 1900 || year > 2100 || month < 1 || month > 12)
            {
                _logger.LogWarning("Invalid date parameters provided: Year {Year}, Month {Month}", year, month);
                return BadRequest("Invalid year or month provided.");
            }

            var summary = await _workSessionService.GetWorkSessionSummaryAsync(studentId, year, month);

            _logger.LogInformation("Successfully retrieved work session summary for student {StudentId}.", studentId);
            return Ok(summary);
        }

        [HttpPut("{id}")] // Get ID from route
        public async Task<IActionResult> UpdateWorkSession(int id, [FromBody] UpdateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to update WorkSession {WorkSessionId}.", authenticatedUserId, id);

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                _logger.LogWarning("User {UserId} failed to update WorkSession {WorkSessionId}. Session not found.", authenticatedUserId, id);
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.IsWorkSessionOwner);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to modify WorkSession {WorkSessionId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to modify this work session.");
            }

            var updatedWorkSession = await _workSessionService.UpdateWorkSessionAsync(workSession, dto);
            if (updatedWorkSession == null)
            {
                _logger.LogError("Failed to update WorkSession {WorkSessionId} for User {UserId} even after authorization.", id, authenticatedUserId);
                return BadRequest("Failed to update work session.");
            }
            _logger.LogInformation("User {UserId} successfully updated WorkSession {WorkSessionId}.", authenticatedUserId, id);
            return Ok(updatedWorkSession);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkSession(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to delete WorkSession {WorkSessionId}.", authenticatedUserId, id);

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                _logger.LogWarning("User {UserId} failed to delete WorkSession {WorkSessionId}. Session not found.", authenticatedUserId, id);
                return NotFound("Work session not found.");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.IsWorkSessionOwner);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete WorkSession {WorkSessionId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to modify this work session.");
            }

            var result = await _workSessionService.DeleteWorkSessionAsync(workSession);
            if (!result)
            {
                _logger.LogError("Failed to delete WorkSession {WorkSessionId} for User {UserId} even after authorization.", id, authenticatedUserId);
                return BadRequest("Failed to delete work session.");
            }
            _logger.LogInformation("User {UserId} successfully deleted WorkSession {WorkSessionId}.", authenticatedUserId, id);
            return NoContent();
        }

        [HttpPost("verify/{id}")] // Changed to POST, ID from route
        [Authorize(Policy = Policies.CompanyOnly)]
        public async Task<IActionResult> VerifyStudentWorkSession(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("Company user {UserId} attempting to verify WorkSession {WorkSessionId}.", authenticatedUserId, id);

            // Fetch the work session to check supervisor relationship
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                _logger.LogWarning("Company user {UserId} failed to verify WorkSession {WorkSessionId}. Session not found.", authenticatedUserId, id);
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.CanVerifyWorkSession);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("Company user {UserId} is not authorized to verify WorkSession {WorkSessionId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to verify this student's work session.");
            }

            var verifiedWorkSession = await _workSessionService.VerifyWorkSessionAsync(workSession);
            if (verifiedWorkSession == null)
            {
                _logger.LogError("Failed to verify WorkSession {WorkSessionId} for company user {UserId} even after authorization.", id, authenticatedUserId);
                return BadRequest("Failed to verify work session.");
            }
            _logger.LogInformation("Company user {UserId} successfully verified WorkSession {WorkSessionId}.", authenticatedUserId, id);
            return Ok(verifiedWorkSession);
        }
    }
}