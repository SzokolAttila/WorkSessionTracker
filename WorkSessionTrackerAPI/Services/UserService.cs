using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using OtpNet;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data; // Added for ApplicationDbContext injection

namespace WorkSessionTrackerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context; // Inject DbContext directly for specific queries like Include

        public UserService(IUserRepository userRepository, IEmailService emailService, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _context = context;
        }

        public async Task<Employee?> RegisterEmployeeAsync(RegisterUserDto dto)
        {
            if (await _userRepository.GetUserByEmailAsync(dto.Email) != null)
            {
                return null; // Email already exists
            }

            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24), // Token valid for 24 hours
                EmailVerified = false
            };

            await _userRepository.AddAsync(employee);
            await _emailService.SendEmailAsync(employee.Email, "Verify Your Email",
                $"Please verify your email using this token: {employee.EmailVerificationToken}");

            return employee;
        }

        public async Task<Supervisor?> RegisterSupervisorAsync(RegisterUserDto dto)
        {
            if (await _userRepository.GetUserByEmailAsync(dto.Email) != null)
            {
                return null; // Email already exists
            }

            var supervisor = new Supervisor
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24),
                EmailVerified = false,
                TotpSeed = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)) // Generate a random TOTP seed
            };

            await _userRepository.AddAsync(supervisor);
            await _emailService.SendEmailAsync(supervisor.Email, "Verify Your Email",
                $"Please verify your email using this token: {supervisor.EmailVerificationToken}");

            return supervisor;
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null || user.EmailVerified || user.EmailVerificationToken != dto.Token ||
                user.EmailVerificationTokenExpiration < DateTime.UtcNow)
            {
                return false;
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null; // Clear token after verification
            user.EmailVerificationTokenExpiration = null;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ResendEmailVerificationAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.EmailVerified)
            {
                return false;
            }

            user.EmailVerificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24);
            await _userRepository.UpdateAsync(user);
            await _emailService.SendEmailAsync(user.Email, "Resend Email Verification",
                $"Please verify your email using this new token: {user.EmailVerificationToken}");
            return true;
        }

        public async Task<string?> GenerateTotpSetupCodeAsync(int supervisorId)
        {
            var supervisor = await _userRepository.GetSupervisorByIdAsync(supervisorId);
            if (supervisor == null || string.IsNullOrEmpty(supervisor.TotpSeed))
            {
                return null;
            }

            // In a real application, you'd generate a QR code URI here.
            // Example: otpauth://totp/WorkSessionTracker:{supervisor.Email}?secret={supervisor.TotpSeed}&issuer=WorkSessionTracker
            return $"TOTP Secret for {supervisor.Email}: {supervisor.TotpSeed}";
        }

        public async Task<bool> ConnectEmployeeToSupervisorAsync(ConnectEmployeeToSupervisorDto dto)
        {
            var employee = await _userRepository.GetEmployeeByIdAsync(dto.EmployeeId);
            var supervisor = await _userRepository.GetSupervisorByIdAsync(dto.SupervisorId);

            if (employee == null || supervisor == null || string.IsNullOrEmpty(supervisor.TotpSeed))
            {
                return false;
            }

            // Verify TOTP code
            var totp = new Totp(Base32Encoding.ToBytes(supervisor.TotpSeed));
            if (!totp.VerifyTotp(dto.TotpCode, out _))
            {
                return false; // TOTP verification failed
            }

            employee.SupervisorId = supervisor.Id;
            await _userRepository.UpdateAsync(employee);
            return true;
        }

        public async Task<Supervisor?> GetSupervisorWithEmployeesAsync(int supervisorId)
        {
            return await _context.Supervisors
                                 .Include(s => s.Employees)
                                 .FirstOrDefaultAsync(s => s.Id == supervisorId);
        }
    }
}
