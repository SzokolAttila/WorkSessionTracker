using Microsoft.AspNetCore.Mvc;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
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

        [HttpGet("supervisor/{supervisorId}/totp-setup")]
        public async Task<IActionResult> GetTotpSetupCode(int supervisorId)
        {
            var setupCode = await _userService.GenerateTotpSetupCodeAsync(supervisorId);
            if (setupCode == null)
            {
                return NotFound("Supervisor not found or TOTP not configured.");
            }
            // In a real app, you'd return a QR code image or URI
            return Ok(new { TotpSetupInfo = setupCode });
        }

        [HttpPost("connect-employee-to-supervisor")]
        public async Task<IActionResult> ConnectEmployeeToSupervisor([FromBody] ConnectEmployeeToSupervisorDto dto)
        {
            var result = await _userService.ConnectEmployeeToSupervisorAsync(dto);
            if (!result)
            {
                return BadRequest("Failed to connect employee to supervisor. Check IDs and TOTP code.");
            }
            return Ok("Employee successfully connected to supervisor.");
        }

        [HttpGet("supervisor/{supervisorId}/with-employees")]
        public async Task<IActionResult> GetSupervisorWithEmployees(int supervisorId)
        {
            var supervisor = await _userService.GetSupervisorWithEmployeesAsync(supervisorId);
            if (supervisor == null) return NotFound("Supervisor not found.");
            return Ok(supervisor);
        }
    }
}
