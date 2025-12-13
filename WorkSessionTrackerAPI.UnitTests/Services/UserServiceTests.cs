using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OtpNet;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Services;

namespace WorkSessionTrackerAPI.UnitTests.Services
{
    public class UserServiceTests
    {

        public UserServiceTests()
        {
        }

        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }

        private Mock<UserManager<User>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task GetCompanyWithStudentsAsync_WhenCompanyExists_ShouldReturnCompanyWithStudents()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var company = new Company { Name = "Test Company" };
            var student1 = new Student { Name = "Student One", Company = company, Email = "s1@test.com", UserName = "s1@test.com" };
            var student2 = new Student { Name = "Student Two", Company = company, Email = "s2@test.com", UserName = "s2@test.com" };
            dbContext.AddRange(student1, student2);
            await dbContext.SaveChangesAsync();

            var userManagerMock = CreateUserManagerMock();
            var service = new UserService(userManagerMock.Object, dbContext);

            // Act
            var result = await service.GetCompanyWithStudentsAsync(company.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(company.Id);
            result.Students.Should().HaveCount(2);
            result.Students.Select(s => s.Id).Should().Contain(new[] { student1.Id, student2.Id });
        }

        [Fact]
        public async Task GetCompanyWithStudentsAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var userManagerMock = CreateUserManagerMock();
            var service = new UserService(userManagerMock.Object, dbContext);
            var nonExistentCompanyId = 999;

            // Act
            var result = await service.GetCompanyWithStudentsAsync(nonExistentCompanyId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GenerateTotpSetupCodeForCompanyAsync_WhenCompanyExistsAndHasNoKey_ShouldResetKeyAndReturnCode()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var company = new Company { Id = 1, Name = "Test Company" };
            var newKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)); // A valid key

            userManagerMock.Setup(um => um.FindByIdAsync(company.Id.ToString())).ReturnsAsync(company);
            // Simulate key not existing initially, then existing after reset
            userManagerMock.SetupSequence(um => um.GetAuthenticatorKeyAsync(company))
                           .ReturnsAsync((string)null)
                           .ReturnsAsync(newKey);
            userManagerMock.Setup(um => um.ResetAuthenticatorKeyAsync(company)).ReturnsAsync(IdentityResult.Success);

            var dbContext = await GetDbContext();
            var service = new UserService(userManagerMock.Object, dbContext);

            // Act
            var result = await service.GenerateTotpSetupCodeForCompanyAsync(company.Id);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Length.Should().Be(6); // TOTP codes are 6 digits
            userManagerMock.Verify(um => um.ResetAuthenticatorKeyAsync(company), Times.Once);
        }

        [Fact]
        public async Task GenerateTotpSetupCodeForCompanyAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var nonExistentCompanyId = 999;
            userManagerMock.Setup(um => um.FindByIdAsync(nonExistentCompanyId.ToString())).ReturnsAsync((User)null);

            var dbContext = await GetDbContext();
            var service = new UserService(userManagerMock.Object, dbContext);

            // Act
            var result = await service.GenerateTotpSetupCodeForCompanyAsync(nonExistentCompanyId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ConnectStudentToCompanyAsync_WithValidTotp_ShouldConnectStudentAndReturnTrue()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var company = new Company { Name = "Test Company" };
            var student = new Student { Name = "Test Student", Email = "s@test.com", UserName = "s@test.com", CompanyId = null };
            dbContext.Add(company);
            dbContext.Add(student);
            await dbContext.SaveChangesAsync();

            var userManagerMock = CreateUserManagerMock();
            var dto = new StudentConnectToCompanyDto { CompanyId = company.Id, TotpCode = "123456" };

            userManagerMock.Setup(um => um.FindByIdAsync(dto.CompanyId.ToString())).ReturnsAsync(company);
            userManagerMock.Setup(um => um.VerifyTwoFactorTokenAsync(company, "Authenticator", dto.TotpCode)).ReturnsAsync(true);

            var service = new UserService(userManagerMock.Object, dbContext);

            // Act
            var result = await service.ConnectStudentToCompanyAsync(student.Id, dto);

            // Assert
            result.Should().BeTrue();
            var updatedStudent = await dbContext.Users.OfType<Student>().FirstAsync(s => s.Id == student.Id);
            updatedStudent.CompanyId.Should().Be(company.Id);
        }

        [Fact]
        public async Task ConnectStudentToCompanyAsync_WithInvalidTotp_ShouldNotConnectStudentAndReturnFalse()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var company = new Company { Name = "Test Company" };
            var student = new Student { Name = "Test Student", Email = "s@test.com", UserName = "s@test.com", CompanyId = null };
            dbContext.Add(company);
            dbContext.Add(student);
            await dbContext.SaveChangesAsync();

            var userManagerMock = CreateUserManagerMock();
            var dto = new StudentConnectToCompanyDto { CompanyId = company.Id, TotpCode = "654321" };

            userManagerMock.Setup(um => um.FindByIdAsync(dto.CompanyId.ToString())).ReturnsAsync(company);
            userManagerMock.Setup(um => um.VerifyTwoFactorTokenAsync(company, "Authenticator", dto.TotpCode)).ReturnsAsync(false);

            var service = new UserService(userManagerMock.Object, dbContext);

            // Act
            var result = await service.ConnectStudentToCompanyAsync(student.Id, dto);

            // Assert
            result.Should().BeFalse();
            var updatedStudent = await dbContext.Users.OfType<Student>().FirstAsync(s => s.Id == student.Id);
            updatedStudent.CompanyId.Should().BeNull();
        }
    }
}
