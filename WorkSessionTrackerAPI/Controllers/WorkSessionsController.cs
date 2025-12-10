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

        private readonly IWorkSessionRepository _workSessionRepository; // Added for direct GetByIdAsync calls
        public WorkSessionsController(IWorkSessionService workSessionService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository, IAuthorizationService authorizationService) : base() // Call base constructor
        {
            _workSessionService = workSessionService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Authorize(Policy = Policies.StudentOnly)]
        public async Task<IActionResult> CreateWorkSession([FromBody] CreateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

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
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, studentId, Policies.CanAccessStudentData);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to view these work sessions.");
            }

            var workSessions = await _workSessionService.GetStudentWorkSessionsAsync(studentId);
            return Ok(workSessions);
        }

        [HttpPut("{id}")] // Get ID from route
        public async Task<IActionResult> UpdateWorkSession(int id, [FromBody] UpdateWorkSessionDto dto)
        {
            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.IsWorkSessionOwner);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to modify this work session.");
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
            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.IsWorkSessionOwner);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to modify this work session.");
            }

            var result = await _workSessionService.DeleteWorkSessionAsync(workSession);
            if (!result)
            {
                return BadRequest("Failed to delete work session.");
            }
            return NoContent();
        }

        [HttpPost("verify/{id}")] // Changed to POST, ID from route
        [Authorize(Policy = Policies.CompanyOnly)]
        public async Task<IActionResult> VerifyStudentWorkSession(int id)
        {
            // Fetch the work session to check supervisor relationship
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.CanVerifyWorkSession);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to verify this student's work session.");
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