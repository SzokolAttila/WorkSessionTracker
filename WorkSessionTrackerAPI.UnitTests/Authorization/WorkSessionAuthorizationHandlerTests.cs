using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Authorization;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.UnitTests.Authorization
{
    public class WorkSessionAuthorizationHandlerTests
    {
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }

        private ClaimsPrincipal CreateUser(string id, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private async Task<(int studentId, int companyId, int workSessionId)> SeedData(ApplicationDbContext dbContext)
        {
            var company = new Company { Name = "Test Inc." };
            var student = new Student { Name = "Test Student", Company = company, Email = "student@test.com", UserName = "student@test.com" };
            var workSession = new WorkSession { Student = student, StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow.AddHours(1) };

            dbContext.Add(workSession);
            await dbContext.SaveChangesAsync();

            return (student.Id, company.Id, workSession.Id);
        }

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenUserIsAdmin()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var (_, _, workSessionId) = await SeedData(dbContext);
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new CanVerifyWorkSessionRequirement();
            var adminUser = CreateUser("99", UserRoleEnum.Admin.ToString());
            var workSession = await dbContext.WorkSessions.FindAsync(workSessionId);
            var context = new AuthorizationHandlerContext(new[] { requirement }, adminUser, workSession);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenStudentAccessesOwnWorkSession()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var (studentId, _, workSessionId) = await SeedData(dbContext);
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new IsWorkSessionOwnerRequirement();
            var studentUser = CreateUser(studentId.ToString(), UserRoleEnum.Student.ToString());
            var workSession = await dbContext.WorkSessions.FindAsync(workSessionId);
            var context = new AuthorizationHandlerContext(new[] { requirement }, studentUser, workSession);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenCompanyAccessesTheirStudentsWorkSession()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var (_, companyId, workSessionId) = await SeedData(dbContext);
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new CanVerifyWorkSessionRequirement();
            var companyUser = CreateUser(companyId.ToString(), UserRoleEnum.Company.ToString());
            var workSession = await dbContext.WorkSessions.FindAsync(workSessionId);
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, workSession);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenStudentAccessesAnotherStudentsWorkSession()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var (_, _, workSessionId) = await SeedData(dbContext);
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new IsWorkSessionOwnerRequirement();
            var otherStudentId = 99;
            var requestingStudent = CreateUser(otherStudentId.ToString(), UserRoleEnum.Student.ToString());
            var workSession = await dbContext.WorkSessions.FindAsync(workSessionId);
            var context = new AuthorizationHandlerContext(new[] { requirement }, requestingStudent, workSession);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenCompanyAccessesUnrelatedStudentsWorkSession()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var (_, _, workSessionId) = await SeedData(dbContext);
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new CanVerifyWorkSessionRequirement();
            var otherCompany = new Company { Name = "Other Company" };
            dbContext.Add(otherCompany);
            await dbContext.SaveChangesAsync();
            var companyUser = CreateUser(otherCompany.Id.ToString(), UserRoleEnum.Company.ToString());
            var workSession = await dbContext.WorkSessions.FindAsync(workSessionId);
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, workSession);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenResourceIsNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new WorkSessionAuthorizationHandler(dbContext);
            var requirement = new IsWorkSessionOwnerRequirement();
            var studentUser = CreateUser("1", UserRoleEnum.Student.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, studentUser, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}
