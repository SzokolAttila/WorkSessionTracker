using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using OtpNet; // Assuming you have this for TOTP

namespace WorkSessionTrackerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context; // For accessing other entities and for TPH casting

        public UserService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Company?> GetCompanyWithStudentsAsync(int companyId)
        {
            // Use UserManager to find the user, then attempt to cast to Company
            // Eager load students if Company model has a navigation property for them
            // This assumes your Company model has a 'public ICollection<Student> Students { get; set; }'
            var company = await _context.Users
                                           .OfType<Company>()
                                           .Include(s => s.Students)
                                           .FirstOrDefaultAsync(s => s.Id == companyId);
            return company;
        }

        public async Task<string?> GenerateTotpSetupCodeForCompanyAsync(int companyId)
        {
            var company = await _userManager.FindByIdAsync(companyId.ToString());
            if (company is null)
            {
                return null;
            }

            // Use Identity's built-in authenticator key management for better security and integration.
            var totpKey = await _userManager.GetAuthenticatorKeyAsync(company);
            if (string.IsNullOrEmpty(totpKey))
            {
                // This generates a new key and stores it in the user's record.
                await _userManager.ResetAuthenticatorKeyAsync(company);
                totpKey = await _userManager.GetAuthenticatorKeyAsync(company);
            }

            var totp = new Totp(Base32Encoding.ToBytes(totpKey));
            return totp.ComputeTotp(); // Return current TOTP code
        }

        public async Task<bool> ConnectStudentToCompanyAsync(int studentId, StudentConnectToCompanyDto dto)
        {
            var student = await _context.Users.OfType<Student>().FirstOrDefaultAsync(s => s.Id == studentId);
            var company = await _userManager.FindByIdAsync(dto.CompanyId.ToString());

            if (student is null || company is null)
            {
                return false;
            }

            // Use Identity's built-in TOTP verification. This is more secure and handles time windows correctly.
            var isTotpValid = await _userManager.VerifyTwoFactorTokenAsync(company, _userManager.Options.Tokens.AuthenticatorTokenProvider, dto.TotpCode);

            if (!isTotpValid)
            {
                return false; // Invalid TOTP code
            }

            // This assumes your Student model has a 'public int? CompanyId { get; set; }' property
            student.CompanyId = company.Id;
            _context.Users.Update(student);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}