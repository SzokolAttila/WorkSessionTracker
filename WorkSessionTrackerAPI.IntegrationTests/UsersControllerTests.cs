using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly Func<Task> _resetDatabase;

        // User and entity IDs for tests
        private int _studentId;
        private const string StudentEmail = "student.user@example.com";
        private const string StudentPassword = "Password123!";

        private int _companyId;
        private const string CompanyEmail = "company.user@example.com";
        private const string CompanyPassword = "Password123!";

        private int _otherCompanyId;
        private const string OtherCompanyEmail = "other.company.user@example.com";
        private const string OtherCompanyPassword = "Password123!";

        private int _unassignedStudentId;
        private const string UnassignedStudentEmail = "unassigned.student@example.com";
        private const string UnassignedStudentPassword = "Password123!";

        public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
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

            // Create Unassigned Student for connection tests
            var unassignedStudent = new Student { UserName = UnassignedStudentEmail, Email = UnassignedStudentEmail, Name = "Unassigned Student" };
            await userManager.CreateAsync(unassignedStudent, UnassignedStudentPassword);
            await userManager.AddToRoleAsync(unassignedStudent, "Student");
            var unassignedStudentToken = await userManager.GenerateEmailConfirmationTokenAsync(unassignedStudent);
            await userManager.ConfirmEmailAsync(unassignedStudent, unassignedStudentToken);
            _unassignedStudentId = unassignedStudent.Id;

            // Connect student to company and save changes
            student.CompanyId = _companyId;
            await context.SaveChangesAsync();
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
        public async Task GetCompanyWithStudents_AsCompany_ReturnsOkAndListOfStudents()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);

            // Act
            var response = await authClient.GetAsync("/api/users/company/with-students");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var company = await response.Content.ReadFromJsonAsync<Company>();
            company.Should().NotBeNull();
            // Using ContainSingle is more robust than checking count and then the first element.
            company.Students.Should().ContainSingle(s => s.Id == _studentId);
        }

        [Fact]
        public async Task GetCompanyWithStudents_AsStudent_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await authClient.GetAsync("/api/users/company/with-students");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetCompanyTotpSetupCode_AsCompany_ReturnsOk()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);

            // Act
            var response = await authClient.GetAsync("/api/users/company/totp-setup");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            responseData.GetProperty("totpCode").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetCompanyTotpSetupCode_AsStudent_ReturnsForbidden()
        {
            // Arrange
            var authClient = await GetAuthenticatedClientAsync(StudentEmail, StudentPassword);

            // Act
            var response = await authClient.GetAsync("/api/users/company/totp-setup");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ConnectStudentToCompany_WithValidTotp_ReturnsOk()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var studentClient = await GetAuthenticatedClientAsync(UnassignedStudentEmail, UnassignedStudentPassword);

            var totpResponse = await companyClient.GetAsync("/api/users/company/totp-setup");
            totpResponse.EnsureSuccessStatusCode();
            var totpData = await totpResponse.Content.ReadFromJsonAsync<JsonElement>();
            var totpCode = totpData.GetProperty("totpCode").GetString();

            var connectDto = new StudentConnectToCompanyDto { CompanyId = _companyId, TotpCode = totpCode };

            // Act
            var connectResponse = await studentClient.PostAsJsonAsync("/api/users/connect-student-to-company", connectDto);

            // Assert
            connectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ConnectStudentToCompany_AsCompany_ReturnsForbidden()
        {
            // Arrange
            var companyClient = await GetAuthenticatedClientAsync(CompanyEmail, CompanyPassword);
            var connectDto = new StudentConnectToCompanyDto { CompanyId = _companyId, TotpCode = "123456" };

            // Act
            var response = await companyClient.PostAsJsonAsync("/api/users/connect-student-to-company", connectDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}