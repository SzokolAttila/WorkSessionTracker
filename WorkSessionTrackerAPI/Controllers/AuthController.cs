using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Interfaces; // For IEmailService
using Microsoft.Extensions.Logging;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService; // Assuming you have an email service for sending confirmation emails
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        private bool IsCompany(string role) => string.Equals(role, UserRoleEnum.Company.ToString(), StringComparison.OrdinalIgnoreCase);
        private bool IsStudent(string role) => string.Equals(role, UserRoleEnum.Student.ToString(), StringComparison.OrdinalIgnoreCase);
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            _logger.LogInformation("Registration attempt for email {Email}", model.Email);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration failed for email {Email} due to invalid model state.", model.Email);
                return BadRequest(ModelState);
            }

            // The RegisterRequestDto should include a 'Role' property to specify user type.
            // We are assuming 'model.Role' is a string like "Student" or "Company".
            User user;
            string role;

            if (IsCompany(model.Role))
            {
                user = new Company();
                role = UserRoleEnum.Company.ToString();
            }
            else if (IsStudent(model.Role))
            {
                user = new Student();
                role = UserRoleEnum.Student.ToString();
            }
            else
            {
                _logger.LogWarning("Registration failed for email {Email}. Invalid role '{Role}' specified.", model.Email, model.Role);
                return BadRequest("Invalid role specified. Must be 'Student' or 'Company'.");
            }

            user.UserName = model.Email;
            user.Email = model.Email;
            user.Name = model.DisplayName;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                await _emailService.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>");

                _logger.LogInformation("User {Email} registered successfully as role {Role}. Awaiting email confirmation.", user.Email, role);
                return Ok(new { Message = "User registered successfully. Please confirm your email." });
            }

            _logger.LogWarning("User registration failed for {Email}. Errors: {Errors}", model.Email, result.Errors);
            return BadRequest(result.Errors);
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            _logger.LogInformation("Email confirmation attempt for User ID {UserId}", userId);
            if (userId == 0 || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Email confirmation failed due to invalid request parameters for User ID {UserId}", userId);
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed. User not found for User ID {UserId}", userId);
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for User {Email} (ID: {UserId})", user.Email, userId);
                return Ok("Email confirmed successfully. You can now log in.");
            }

            _logger.LogWarning("Email confirmation failed for User {Email} (ID: {UserId}). Errors: {Errors}", user.Email, userId, result.Errors);
            return BadRequest("Error confirming your email.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            _logger.LogInformation("Login attempt for user {Email}", model.Email);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login failed for {Email} due to invalid model state.", model.Email);
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed for {Email}. User not found.", model.Email);
                return Unauthorized("Invalid credentials.");
            }

            // Use SignInManager to handle login attempts. It correctly handles lockout,
            // two-factor authentication, and checks for confirmed email if configured.
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", model.Email);
                var token = await GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account is locked out.", model.Email);
                return Unauthorized("Account locked out.");
            }
            if (result.IsNotAllowed)
            {
                _logger.LogWarning("User {Email} is not allowed to sign in. Email may not be confirmed.", model.Email);
                return Unauthorized("Account not allowed to sign in. Please confirm your email.");
            }

            _logger.LogWarning("Login failed for {Email} due to invalid credentials.", model.Email);
            return Unauthorized("Invalid credentials.");
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // The 'sub' (subject) claim is the standard for the user's unique ID.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // A unique token ID.
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty) // The user's email.
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7")); // Use UtcNow for consistency across timezones

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
