using Microsoft.AspNetCore.Mvc;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using Microsoft.AspNetCore.Authorization; // Add this using statement
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.Authorization;
using WorkSessionTrackerAPI.Models; // Add this for User
using Microsoft.Extensions.Logging;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints in this controller now require authentication
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, UserManager<User> userManager, ILogger<UsersController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("company/totp-setup")]
        [Authorize(Policy = Policies.CompanyOnly)] // Only companies can access this
        public async Task<IActionResult> GetCompanyTotpSetupCode()
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("Company user {UserId} requesting TOTP setup code.", authenticatedUserId);

            var setupCode = await _userService.GenerateTotpSetupCodeForCompanyAsync(authenticatedUserId);
            if (setupCode == null)
            {
                _logger.LogWarning("TOTP setup code generation failed for company user {UserId}. User not found or not a company.", authenticatedUserId);
                return NotFound("User not found or TOTP not configured for this company.");
            }
            _logger.LogInformation("Successfully generated TOTP setup code for company user {UserId}.", authenticatedUserId);
            // Return the current 6-digit TOTP code
            return Ok(new { TotpCode = setupCode });
        }

        [HttpPost("connect-student-to-company")]
        [Authorize(Policy = Policies.StudentOnly)]
        public async Task<IActionResult> ConnectStudentToCompany([FromBody] StudentConnectToCompanyDto dto)
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("Student {StudentId} attempting to connect to Company {CompanyId}.", authenticatedUserId, dto.CompanyId);

            var result = await _userService.ConnectStudentToCompanyAsync(authenticatedUserId, dto);
            if (!result)
            {
                _logger.LogWarning("Failed to connect student {StudentId} to company {CompanyId}. Check Company ID and TOTP code.", authenticatedUserId, dto.CompanyId);
                return BadRequest("Failed to connect student to company. Check Company ID and TOTP code.");
            }
            _logger.LogInformation("Student {StudentId} successfully connected to Company {CompanyId}.", authenticatedUserId, dto.CompanyId);
            return Ok("Student successfully connected to company.");
        }

        [HttpGet("company/with-students")]
        [Authorize(Policy = Policies.CompanyOnly)] // Only companies can access this
        public async Task<IActionResult> GetCompanyWithStudents()
        {
            // Get the ID of the authenticated user
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("Company user {UserId} requesting list of their students.", authenticatedUserId);

            var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);
            if (company == null)
            {
                _logger.LogWarning("Could not find company data for user {UserId}.", authenticatedUserId);
                return NotFound("Company not found.");
            }
            _logger.LogInformation("Successfully retrieved student list for company user {UserId}.", authenticatedUserId);
            return Ok(company);
        }
    }
}
