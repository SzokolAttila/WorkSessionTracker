using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IUserService
    {
        Task<Employee?> RegisterEmployeeAsync(RegisterUserDto dto);
        Task<Supervisor?> RegisterSupervisorAsync(RegisterUserDto dto);
        Task<bool> VerifyEmailAsync(VerifyEmailDto dto);
        Task<string?> GenerateTotpSetupCodeAsync(int supervisorId); // Returns QR code data or similar
        Task<bool> ConnectEmployeeToSupervisorAsync(ConnectEmployeeToSupervisorDto dto);
        Task<bool> ResendEmailVerificationAsync(int userId);
        Task<Supervisor?> GetSupervisorWithEmployeesAsync(int supervisorId);
        Task<string?> LoginAsync(LoginDto dto); // Returns JWT token on success
    }
}
