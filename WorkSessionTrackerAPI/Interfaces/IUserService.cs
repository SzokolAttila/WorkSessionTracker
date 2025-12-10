using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IUserService
    {
        Task<string?> GenerateTotpSetupCodeForCompanyAsync(int companyId);
        Task<bool> ConnectStudentToCompanyAsync(int studentId, StudentConnectToCompanyDto dto);
        Task<Company?> GetCompanyWithStudentsAsync(int companyId);
    }
}