using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;
using Xunit;

namespace WorkSessionTrackerAPI.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly Func<Task> _resetDatabase;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _resetDatabase = async () =>
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                // Clear dependent tables first to ensure proper test isolation.
                context.Comments.RemoveRange(context.Comments);
                context.WorkSessions.RemoveRange(context.WorkSessions);
                context.Users.RemoveRange(context.Users);
                await context.SaveChangesAsync();

                // This call is problematic and can cause "logger is frozen" errors. It's not needed for the in-memory provider.

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                foreach (var roleName in Enum.GetNames(typeof(UserRoleEnum)))
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            };
        }

        public Task InitializeAsync() => _resetDatabase();

        public Task DisposeAsync() => Task.CompletedTask;

        [Theory]
        [InlineData(nameof(UserRoleEnum.Student))]
        [InlineData(nameof(UserRoleEnum.Company))]
        public async Task Register_WithValidModel_ReturnsOk(string role)
        {
            // Arrange
            var model = new RegisterRequestDto
            {
                Email = $"testuser_{role.ToLower()}@example.com",
                Password = "Password123!",
                DisplayName = $"Test {role}",
                Role = role
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            responseJson.GetProperty("message").GetString().Should().Be("User registered successfully. Please confirm your email.");
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterRequestDto
            {
                Email = "duplicate@example.com",
                Password = "Password123!",
                DisplayName = "Test User",
                Role = "Student"
            };
            await _client.PostAsJsonAsync("/api/auth/register", model);

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var errors = await response.Content.ReadFromJsonAsync<IdentityError[]>();
            errors.Should().Contain(e => e.Code == "DuplicateUserName");
        }

        [Fact]
        public async Task Login_WithValidCredentialsAfterEmailConfirmation_ReturnsOkWithToken()
        {
            // Arrange
            var email = "logintest@example.com";
            var password = "Password123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var user = new Student { UserName = email, Email = email, Name = "Login Test" };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, "Student");
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);
            }

            var loginDto = new LoginRequestDto { Email = email, Password = password };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            responseData.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithUnconfirmedEmail_ReturnsUnauthorized()
        {
            // Arrange
            var registerDto = new RegisterRequestDto { Email = "unconfirmed@example.com", Password = "Password123!", DisplayName = "Unconfirmed", Role = "Student" };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginRequestDto { Email = registerDto.Email, Password = registerDto.Password };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Be("Account not allowed to sign in. Please confirm your email.");
        }

        [Fact]
        public async Task ConfirmEmail_WithValidToken_ReturnsOkAndAllowsLogin()
        {
            // Arrange
            var email = "confirmme@example.com";
            var password = "Password123!";
            string userId;
            string token;

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var user = new Student { UserName = email, Email = email, Name = "Confirm Me" };
                await userManager.CreateAsync(user, password);
                userId = user.Id.ToString();
                token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            }

            // Act
            var response = await _client.GetAsync($"/api/auth/confirm-email?userId={userId}&token={System.Net.WebUtility.UrlEncode(token)}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Be("Email confirmed successfully. You can now log in.");

            // Verify user can now log in
            var loginDto = new LoginRequestDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
