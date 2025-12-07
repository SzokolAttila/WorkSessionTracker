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
        private readonly IUserRepository _userRepository; // Needed to check supervisor relationship

        public WorkSessionService(IWorkSessionRepository workSessionRepository, IUserRepository userRepository)
        {
            _workSessionRepository = workSessionRepository;
            _userRepository = userRepository;
        }

        public async Task<WorkSession?> CreateWorkSessionAsync(CreateWorkSessionDto dto, int employeeId) // Employee existence check is now in the controller
        { // Employee existence check is now in the controller
            var workSession = new WorkSession
            {
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Description = dto.Description,
                EmployeeId = employeeId,
                Verified = false // New work sessions are unverified by default
            };

            await _workSessionRepository.AddAsync(workSession);
            return workSession;
        }

        public async Task<IEnumerable<WorkSession>> GetEmployeeWorkSessionsAsync(int employeeId) // Authorization is now in the controller
        {
            return await _workSessionRepository.GetWorkSessionsByEmployeeIdAsync(employeeId);
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
