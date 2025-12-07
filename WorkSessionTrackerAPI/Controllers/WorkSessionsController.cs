using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkSessionsController : BaseApiController // Inherit from BaseApiController
    {
        private readonly IWorkSessionService _workSessionService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository; // Needed for supervisor authorization checks
        private readonly IWorkSessionRepository _workSessionRepository; // Added for direct GetByIdAsync calls
        public WorkSessionsController(IWorkSessionService workSessionService, IUserService userService, IUserRepository userRepository, IWorkSessionRepository workSessionRepository) : base() // Call base constructor
        {
            _workSessionService = workSessionService;
            _userService = userService;
            _userRepository = userRepository;
            _workSessionRepository = workSessionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkSession([FromBody] CreateWorkSessionDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Authorization: Only an Employee can create a work session for themselves
            if (authenticatedUserRole != "Employee")
            {
                return Forbid("Only employees can create work sessions.");
            }

            // Ensure the authenticated user is actually an employee (redundant if role check is strict, but good for safety)
            var employee = await _userRepository.GetEmployeeByIdAsync(authenticatedUserId);
            if (employee == null)
            {
                return Forbid("Authenticated user is not recognized as an employee.");
            }

            var workSession = await _workSessionService.CreateWorkSessionAsync(dto, authenticatedUserId);
            if (workSession == null)
            {
                return BadRequest("Could not create work session. Ensure employee exists.");
            }
            return Ok(workSession); // Changed from CreatedAtAction to Ok
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetWorkSessionsForEmployee(int employeeId)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Authorization logic moved to controller
            if (employeeId == authenticatedUserId)
            {
                // Employee viewing their own work sessions
                var workSessions = await _workSessionService.GetEmployeeWorkSessionsAsync(employeeId);
                return Ok(workSessions);
            }
            else if (authenticatedUserRole == "Supervisor")
            {
                // Supervisor viewing their employee's work sessions
                var supervisor = await _userService.GetSupervisorWithEmployeesAsync(authenticatedUserId);
                if (supervisor != null && supervisor.Employees.Any(e => e.Id == employeeId))
                {
                    var workSessions = await _workSessionService.GetEmployeeWorkSessionsAsync(employeeId);
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
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the employee who owns the work session can update it
            if (authenticatedUserRole != "Employee" || workSession.EmployeeId != authenticatedUserId)
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
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Fetch the work session to check ownership
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the employee who owns the work session can delete it
            if (authenticatedUserRole != "Employee" || workSession.EmployeeId != authenticatedUserId)
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
        public async Task<IActionResult> VerifyWorkSession(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            var authenticatedUserRole = GetAuthenticatedUserRole();

            // Fetch the work session to check supervisor relationship
            var workSession = await _workSessionRepository.GetByIdAsync(id); // Directly use repository's GetByIdAsync
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            // Authorization: Only the supervisor of the employee can verify the work session
            if (authenticatedUserRole != "Supervisor")
            {
                return Forbid("Only supervisors can verify work sessions.");
            }

            var supervisor = await _userService.GetSupervisorWithEmployeesAsync(authenticatedUserId);
            if (supervisor == null || supervisor.Employees.All(e => e.Id != workSession.EmployeeId))
            {
                return Forbid("You are not authorized to verify this employee's work session.");
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