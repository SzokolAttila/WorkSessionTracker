using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;
using Xunit;

namespace WorkSessionTrackerAPI.IntegrationTests
{
    public class WorkSessionsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly Func<Task> _resetDatabase;

        // User and entity IDs for tests
        private int _studentId;
        private const string StudentEmail = "student.ws@example.com";
        private const string StudentPassword = "Password123!";

        private int _companyId;
        private const string CompanyEmail = "company.ws@example.com";
        private const string CompanyPassword = "Password123!";

        private int _otherCompanyId;
        private const string OtherCompanyEmail = "other.company.ws@example.com";
        private const string OtherCompanyPassword = "Password123!";

        private int _workSessionId;

        public WorkSessionsControllerTests(CustomWebApplicationFactory<Program> factory)
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
            _otherCompanyId = otherCompany.Id;

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

        [Fact]
        public async Task CreateWorkSession_AsStudent_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);
            var dto = new CreateWorkSessionDto
            {
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(1),
                Description = "New session"
            };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/worksessions", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var workSession = await response.Content.ReadFromJsonAsync<WorkSession>();
            workSession.Should().NotBeNull();
            workSession.Description.Should().Be("New session");
            workSession.StudentId.Should().Be(_studentId);
        }

        [Fact]
        public async Task CreateWorkSession_AsCompany_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var dto = new CreateWorkSessionDto { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow.AddHours(1) };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/worksessions", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetWorkSessionsForStudent_AsSelf_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await authClient.GetAsync($"/api/worksessions/student/{_studentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var sessions = await response.Content.ReadFromJsonAsync<List<WorkSession>>();
            sessions.Should().HaveCount(1);
            sessions[0].Id.Should().Be(_workSessionId);
        }

        [Fact]
        public async Task GetWorkSessionsForStudent_AsAssociatedCompany_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);

            // Act
            var response = await authClient.GetAsync($"/api/worksessions/student/{_studentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetWorkSessionsForStudent_AsUnrelatedCompany_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(OtherCompanyEmail, OtherCompanyPassword);

            // Act
            var response = await authClient.GetAsync($"/api/worksessions/student/{_studentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateWorkSession_AsOwner_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);
            var dto = new UpdateWorkSessionDto { Description = "Updated description", EndDateTime = DateTime.UtcNow.AddHours(2), StartDateTime = DateTime.UtcNow.AddHours(-1) };

            // Act
            var response = await authClient.PutAsJsonAsync($"/api/worksessions/{_workSessionId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var workSession = await response.Content.ReadFromJsonAsync<WorkSession>();
            workSession.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task UpdateWorkSession_AsNonOwner_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var dto = new UpdateWorkSessionDto { Description = "Company trying to update", StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow.AddHours(1) };

            // Act
            var response = await authClient.PutAsJsonAsync($"/api/worksessions/{_workSessionId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteWorkSession_AsOwner_ReturnsNoContent()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await authClient.DeleteAsync($"/api/worksessions/{_workSessionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task VerifyWorkSession_AsAssociatedCompany_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);

            // Act
            var response = await authClient.PostAsync($"/api/worksessions/verify/{_workSessionId}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var workSession = await response.Content.ReadFromJsonAsync<WorkSession>();
            workSession.Verified.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyWorkSession_AsStudent_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await authClient.PostAsync($"/api/worksessions/verify/{_workSessionId}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task VerifyWorkSession_AsUnrelatedCompany_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(OtherCompanyEmail, OtherCompanyPassword);

            // Act
            var response = await authClient.PostAsync($"/api/worksessions/verify/{_workSessionId}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
