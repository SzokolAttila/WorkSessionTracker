using Microsoft.AspNetCore.Authorization;
using FluentAssertions;
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
    public class StudentDataAuthorizationHandlerTests
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

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenUserIsAdmin()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var adminUser = CreateUser("99", UserRoleEnum.Admin.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, adminUser, 1);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenStudentAccessesOwnData()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var studentId = 1;
            var studentUser = CreateUser(studentId.ToString(), UserRoleEnum.Student.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, studentUser, studentId);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldSucceed_WhenCompanyAccessesTheirStudentData()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var companyId = 2;
            var studentId = 1;
            dbContext.Users.Add(new Student { Id = studentId, CompanyId = companyId, Name = "s", Email = "s@s.com", UserName = "s@s.com" });
            await dbContext.SaveChangesAsync();

            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var companyUser = CreateUser(companyId.ToString(), UserRoleEnum.Company.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, studentId);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenStudentAccessesAnotherStudentsData()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var studentId1 = 1;
            var studentId2 = 2;
            var requestingStudent = CreateUser(studentId1.ToString(), UserRoleEnum.Student.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, requestingStudent, studentId2);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenCompanyAccessesUnrelatedStudentData()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var studentCompanyId = 1;
            var requestingCompanyId = 2;
            var studentId = 3;
            dbContext.Users.Add(new Student { Id = studentId, CompanyId = studentCompanyId, Name = "s", Email = "s@s.com", UserName = "s@s.com" });
            await dbContext.SaveChangesAsync();

            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var companyUser = CreateUser(requestingCompanyId.ToString(), UserRoleEnum.Company.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, studentId);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenStudentNotFound()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var companyId = 1;
            var nonExistentStudentId = 999;
            var companyUser = CreateUser(companyId.ToString(), UserRoleEnum.Company.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, nonExistentStudentId);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_ShouldFail_WhenResourceIsNotAnInteger()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var adminUser = CreateUser("1", UserRoleEnum.Admin.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, adminUser, "not-an-int");

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}