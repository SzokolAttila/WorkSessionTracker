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
            var company = new Company { Name = "Test Company" };
            var student = new Student { Company = company, Name = "s", Email = "s@s.com", UserName = "s@s.com" };
            dbContext.Add(student);
            await dbContext.SaveChangesAsync();

            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var companyUser = CreateUser(company.Id.ToString(), UserRoleEnum.Company.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, student.Id);

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
            var studentCompany = new Company { Name = "Student's Company" };
            var otherCompany = new Company { Name = "Other Company" };
            var student = new Student { Company = studentCompany, Name = "s", Email = "s@s.com", UserName = "s@s.com" };
            dbContext.Add(otherCompany);
            dbContext.Add(student);
            await dbContext.SaveChangesAsync();

            var handler = new StudentDataAuthorizationHandler(dbContext);
            var requirement = new StudentDataRequirement();
            var companyUser = CreateUser(otherCompany.Id.ToString(), UserRoleEnum.Company.ToString());
            var context = new AuthorizationHandlerContext(new[] { requirement }, companyUser, student.Id);

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