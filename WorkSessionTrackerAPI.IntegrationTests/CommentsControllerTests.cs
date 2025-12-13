using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;
using Xunit;

namespace WorkSessionTrackerAPI.IntegrationTests
{
    public class CommentsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly Func<Task> _resetDatabase;

        // User and entity IDs for tests
        private int _studentId;
        private const string StudentEmail = "student.comment@example.com";
        private const string StudentPassword = "Password123!";

        private int _companyId;
        private const string CompanyEmail = "company.comment@example.com";
        private const string CompanyPassword = "Password123!";

        private const string OtherCompanyEmail = "other.company.comment@example.com";
        private const string OtherCompanyPassword = "Password123!";

        private int _workSessionId;

        public CommentsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _resetDatabase = async () =>
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Clear dependent tables first
                context.Comments.RemoveRange(context.Comments);
                context.WorkSessions.RemoveRange(context.WorkSessions);
                context.Users.RemoveRange(context.Users);
                await context.SaveChangesAsync();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                foreach (var roleName in Enum.GetNames(typeof(UserRoleEnum)))
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            };
        }

        public async Task InitializeAsync()
        {
            await _resetDatabase();
            await SeedDatabaseAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create Student
            var student = new Student { UserName = StudentEmail, Email = StudentEmail, Name = "Test Student" };
            await userManager.CreateAsync(student, StudentPassword);
            await userManager.AddToRoleAsync(student, "Student");
            var studentToken = await userManager.GenerateEmailConfirmationTokenAsync(student);
            await userManager.ConfirmEmailAsync(student, studentToken);
            _studentId = student.Id;

            // Create Company
            var company = new Company { UserName = CompanyEmail, Email = CompanyEmail, Name = "Test Company" };
            await userManager.CreateAsync(company, CompanyPassword);
            await userManager.AddToRoleAsync(company, "Company");
            var companyToken = await userManager.GenerateEmailConfirmationTokenAsync(company);
            await userManager.ConfirmEmailAsync(company, companyToken);
            _companyId = company.Id;

            // Create Other Company
            var otherCompany = new Company { UserName = OtherCompanyEmail, Email = OtherCompanyEmail, Name = "Other Company" };
            await userManager.CreateAsync(otherCompany, OtherCompanyPassword);
            await userManager.AddToRoleAsync(otherCompany, "Company");
            var otherCompanyToken = await userManager.GenerateEmailConfirmationTokenAsync(otherCompany);
            await userManager.ConfirmEmailAsync(otherCompany, otherCompanyToken);

            // Connect student to company
            student.CompanyId = _companyId;

            // Create WorkSession
            var workSession = new WorkSession
            {
                StudentId = _studentId,
                StartDateTime = DateTime.UtcNow.AddHours(-2),
                EndDateTime = DateTime.UtcNow.AddHours(-1),
                Description = "Test work session"
            };
            context.WorkSessions.Add(workSession);
            await context.SaveChangesAsync();
            _workSessionId = workSession.Id;
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync(string email, string password)
        {
            var loginDto = new LoginRequestDto { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = responseData.GetProperty("token").GetString();

            var authClient = _factory.CreateClient();
            authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return authClient;
        }

        private async Task<Comment> CreateCommentViaApiAsync(HttpClient client, int workSessionId, string content)
        {
            var dto = new CreateCommentDto { WorkSessionId = workSessionId, Content = content };
            var response = await client.PostAsJsonAsync("/api/comments", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Comment>();
        }

        [Fact]
        public async Task CreateComment_AsAuthorizedCompany_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var dto = new CreateCommentDto { WorkSessionId = _workSessionId, Content = "Good work!" };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/comments", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comment = await response.Content.ReadFromJsonAsync<Comment>();
            comment.Should().NotBeNull();
            comment.Content.Should().Be("Good work!");
            comment.WorkSessionId.Should().Be(_workSessionId);
            comment.CompanyId.Should().Be(_companyId);
        }

        [Fact]
        public async Task CreateComment_AsStudent_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);
            var dto = new CreateCommentDto { WorkSessionId = _workSessionId, Content = "I commented on my own session." };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/comments", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateComment_AsUnauthorizedCompany_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(OtherCompanyEmail, OtherCompanyPassword);
            var dto = new CreateCommentDto { WorkSessionId = _workSessionId, Content = "I'm from another company." };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/comments", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetCommentById_AsWorkSessionOwner_ReturnsOk()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(companyClient, _workSessionId, "A comment to get.");
            var studentClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await studentClient.GetAsync($"/api/comments/{createdComment.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comment = await response.Content.ReadFromJsonAsync<Comment>();
            comment.Should().NotBeNull();
            comment.Id.Should().Be(createdComment.Id);
        }

        [Fact]
        public async Task GetCommentById_AsUnrelatedUser_ReturnsForbidden()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(companyClient, _workSessionId, "A comment to get.");
            var otherCompanyClient = await GetAuthenticatedClientAsync(OtherCompanyEmail, OtherCompanyPassword);

            // Act
            var response = await otherCompanyClient.GetAsync($"/api/comments/{createdComment.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateComment_AsCommentOwner_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(authClient, _workSessionId, "Original content.");
            var dto = new UpdateCommentDto { Content = "Updated content" };

            // Act
            var response = await authClient.PutAsJsonAsync($"/api/comments/{createdComment.Id}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedComment = await response.Content.ReadFromJsonAsync<Comment>();
            updatedComment.Content.Should().Be("Updated content");
        }

        [Fact]
        public async Task UpdateComment_AsNonOwner_ReturnsForbidden()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(companyClient, _workSessionId, "Original content.");
            var studentClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);
            var dto = new UpdateCommentDto { Content = "I tried to update" };

            // Act
            var response = await studentClient.PutAsJsonAsync($"/api/comments/{createdComment.Id}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteComment_AsCommentOwner_ReturnsNoContent()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(authClient, _workSessionId, "A comment to delete.");

            // Act
            var response = await authClient.DeleteAsync($"/api/comments/{createdComment.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await authClient.GetAsync($"/api/comments/{createdComment.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteComment_AsNonOwner_ReturnsForbidden()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var createdComment = await CreateCommentViaApiAsync(companyClient, _workSessionId, "A comment to delete.");
            var studentClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await studentClient.DeleteAsync($"/api/comments/{createdComment.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
