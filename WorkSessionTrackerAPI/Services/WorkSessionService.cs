using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Services
{
    public class WorkSessionService : IWorkSessionService
    {
        private readonly IWorkSessionRepository _workSessionRepository;
        // private readonly IUserRepository _userRepository; // No longer needed, UserManager handles user checks

        public WorkSessionService(IWorkSessionRepository workSessionRepository)
        {
            _workSessionRepository = workSessionRepository;
        }

        public async Task<WorkSession?> CreateWorkSessionAsync(CreateWorkSessionDto dto, int studentId) // Student existence check is now in the controller
        {
            var workSession = new WorkSession
            {
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Description = dto.Description,
                StudentId = studentId, // Renamed from EmployeeId
                Verified = false // New work sessions are unverified by default
            };

            await _workSessionRepository.AddAsync(workSession);
            return workSession;
        }

        public async Task<IEnumerable<WorkSession>> GetStudentWorkSessionsAsync(int studentId) // Authorization is now in the controller
        {
            return await _workSessionRepository.GetWorkSessionsByStudentIdAsync(studentId); // Renamed from GetWorkSessionsByEmployeeIdAsync
        }

        public async Task<WorkSession?> UpdateWorkSessionAsync(WorkSession existingWorkSession, UpdateWorkSessionDto dto) // Authorization is now in the controller
        {
            existingWorkSession.StartDateTime = dto.StartDateTime;
            existingWorkSession.EndDateTime = dto.EndDateTime;
            existingWorkSession.Description = dto.Description;

            await _workSessionRepository.UpdateAsync(existingWorkSession);
            return existingWorkSession;
        }

        public async Task<bool> DeleteWorkSessionAsync(WorkSession existingWorkSession) // Authorization is now in the controller
        {
            await _workSessionRepository.DeleteAsync(existingWorkSession);
            return true;
        }

        public async Task<WorkSession?> VerifyWorkSessionAsync(WorkSession existingWorkSession) // Authorization is now in the controller
        {
            existingWorkSession.Verified = true; // Always set to true for this POST endpoint
            await _workSessionRepository.UpdateAsync(existingWorkSession);
            return existingWorkSession;
        }
    }
}
