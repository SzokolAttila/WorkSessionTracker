using Microsoft.AspNetCore.Mvc;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using Microsoft.AspNetCore.Authorization; // Add this using statement
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.Authorization;
using WorkSessionTrackerAPI.Models; // Add this for User

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints in this controller now require authentication
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager; // Inject UserManager

        public UsersController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet("company/totp-setup")]
        [Authorize(Policy = Policies.CompanyOnly)] // Only companies can access this
        public async Task<IActionResult> GetCompanyTotpSetupCode()
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();

            var setupCode = await _userService.GenerateTotpSetupCodeForCompanyAsync(authenticatedUserId);
            if (setupCode == null)
            {
                return NotFound("User not found or TOTP not configured for this company.");
            }
            // Return the current 6-digit TOTP code
            return Ok(new { TotpCode = setupCode });
        }

        [HttpPost("connect-student-to-company")]
        [Authorize(Policy = Policies.StudentOnly)]
        public async Task<IActionResult> ConnectStudentToCompany([FromBody] StudentConnectToCompanyDto dto)
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();

            var result = await _userService.ConnectStudentToCompanyAsync(authenticatedUserId, dto);
            if (!result)
            {
                return BadRequest("Failed to connect student to company. Check Company ID and TOTP code.");
            }
            return Ok("Student successfully connected to company.");
        }

        [HttpGet("company/with-students")]
        [Authorize(Policy = Policies.CompanyOnly)] // Only companies can access this
        public async Task<IActionResult> GetCompanyWithStudents()
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();

            var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);
            if (company == null) return NotFound("Company not found.");
            return Ok(company);
        }
    }
}
