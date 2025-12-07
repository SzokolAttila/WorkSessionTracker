using Microsoft.AspNetCore.Mvc;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using Microsoft.AspNetCore.Authorization; // Add this using statement
using System.Security.Claims; // Add this using statement for ClaimTypes
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register/employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterUserDto dto)
        {
            var employee = await _userService.RegisterEmployeeAsync(dto);
            if (employee == null)
            {
                return BadRequest("Email already registered.");
            }
            return CreatedAtAction(nameof(RegisterEmployee), new { id = employee.Id }, employee);
        }

        [HttpPost("register/supervisor")]
        public async Task<IActionResult> RegisterSupervisor([FromBody] RegisterUserDto dto)
        {
            var supervisor = await _userService.RegisterSupervisorAsync(dto);
            if (supervisor == null)
            {
                return BadRequest("Email already registered.");
            }
            return CreatedAtAction(nameof(RegisterSupervisor), new { id = supervisor.Id }, supervisor);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var result = await _userService.VerifyEmailAsync(dto);
            if (!result)
            {
                return BadRequest("Invalid or expired token, or email already verified.");
            }
            return Ok("Email verified successfully.");
        }

        [HttpPost("resend-email-verification/{userId}")]
        public async Task<IActionResult> ResendEmailVerification(int userId)
        {
            var result = await _userService.ResendEmailVerificationAsync(userId);
            if (!result)
            {
                return BadRequest("User not found or email already verified.");
            }
            return Ok("Email verification link resent.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _userService.LoginAsync(dto);
            if (token == null)
            {
                return Unauthorized("Invalid credentials or unverified email.");
            }
            return Ok(new { Token = token });
        }

        [Authorize] // Requires authentication
        [HttpGet("supervisor/{supervisorId}/totp-setup")]
        public async Task<IActionResult> GetTotpSetupCode(int supervisorId)
        {
            // Get the ID of the authenticated user
            var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (authenticatedUserIdClaim == null || !int.TryParse(authenticatedUserIdClaim.Value, out int authenticatedUserId))
            {
                return Unauthorized("User ID not found in token.");
            }
            // Ensure the authenticated user is trying to access their own data
            if (authenticatedUserId != supervisorId)
            {
                return Forbid("You are not authorized to access this supervisor's TOTP setup.");
            }

            var setupCode = await _userService.GenerateTotpSetupCodeAsync(supervisorId);
            if (setupCode == null)
            {
                return NotFound("Supervisor not found or TOTP not configured.");
            }
            // Return the current 6-digit TOTP code
            return Ok(new { TotpCode = setupCode });
        }

        [Authorize] // Requires authentication
        [HttpPost("connect-employee-to-supervisor")]
        public async Task<IActionResult> ConnectEmployeeToSupervisor([FromBody] ConnectEmployeeToSupervisorDto dto)
        {
            // Get the ID of the authenticated user
            var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (authenticatedUserIdClaim == null || !int.TryParse(authenticatedUserIdClaim.Value, out int authenticatedUserId))
            {
                return Unauthorized("User ID not found in token.");
            }
            // Ensure the authenticated user is trying to connect themselves
            if (authenticatedUserId != dto.EmployeeId)
            {
                return Forbid("You are not authorized to connect another employee to a supervisor.");
            }

            var result = await _userService.ConnectEmployeeToSupervisorAsync(dto);
            if (!result)
            {
                return BadRequest("Failed to connect employee to supervisor. Check IDs and TOTP code.");
            }
            return Ok("Employee successfully connected to supervisor.");
        }

        [Authorize] // Requires authentication
        [HttpGet("supervisor/{supervisorId}/with-employees")]
        public async Task<IActionResult> GetSupervisorWithEmployees(int supervisorId)
        {
            // Get the ID of the authenticated user
            var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (authenticatedUserIdClaim == null || !int.TryParse(authenticatedUserIdClaim.Value, out int authenticatedUserId))
            {
                return Unauthorized("User ID not found in token.");
            }
            // Ensure the authenticated user is trying to access their own data
            if (authenticatedUserId != supervisorId)
            {
                return Forbid("You are not authorized to view this supervisor's employees.");
            }

            var supervisor = await _userService.GetSupervisorWithEmployeesAsync(supervisorId);
            if (supervisor == null) return NotFound("Supervisor not found.");
            return Ok(supervisor);
        }
    }
}
