using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IWorkSessionService
    {
        Task<WorkSession?> CreateWorkSessionAsync(CreateWorkSessionDto dto, int employeeId);
        Task<IEnumerable<WorkSession>> GetEmployeeWorkSessionsAsync(int employeeId);
        Task<WorkSession?> GetWorkSessionByIdForAuthAsync(int workSessionId);
        Task<WorkSession?> UpdateWorkSessionAsync(WorkSession existingWorkSession, UpdateWorkSessionDto dto);
        Task<bool> DeleteWorkSessionAsync(WorkSession existingWorkSession);
        Task<WorkSession?> VerifyWorkSessionAsync(WorkSession existingWorkSession);
    }
}
