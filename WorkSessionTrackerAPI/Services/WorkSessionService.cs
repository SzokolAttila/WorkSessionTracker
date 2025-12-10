using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Services
{
    public class WorkSessionService : IWorkSessionService
    {
        private readonly IWorkSessionRepository _workSessionRepository;

        public WorkSessionService(IWorkSessionRepository workSessionRepository)
        {
            _workSessionRepository = workSessionRepository;
        }

        public async Task<WorkSession?> CreateWorkSessionAsync(CreateWorkSessionDto dto, int studentId)
        {
            var workSession = new WorkSession
            {
                StartDateTime = dto.StartDateTime.ToUniversalTime(),
                EndDateTime = dto.EndDateTime.ToUniversalTime(),
                Description = dto.Description,
                StudentId = studentId,
                Verified = false
            };

            await _workSessionRepository.CreateAsync(workSession);
            return workSession;
        }

        public async Task<bool> DeleteWorkSessionAsync(WorkSession workSession)
        {
            return await _workSessionRepository.DeleteAsync(workSession);
        }

        public async Task<IEnumerable<WorkSession>> GetStudentWorkSessionsAsync(int studentId)
        {
            return await _workSessionRepository.GetStudentWorkSessionsAsync(studentId);
        }

        public async Task<WorkSessionSummaryDto> GetWorkSessionSummaryAsync(int studentId, int year, int month)
        {
            return await _workSessionRepository.GetWorkSessionSummaryAsync(studentId, year, month);
        }

        public async Task<WorkSession?> UpdateWorkSessionAsync(WorkSession workSession, UpdateWorkSessionDto dto)
        {
            workSession.StartDateTime = dto.StartDateTime.ToUniversalTime();
            workSession.EndDateTime = dto.EndDateTime.ToUniversalTime();
            workSession.Description = dto.Description;
            await _workSessionRepository.UpdateAsync(workSession);
            return workSession;
        }

        public async Task<WorkSession?> VerifyWorkSessionAsync(WorkSession workSession)
        {
            workSession.Verified = true;
            await _workSessionRepository.UpdateAsync(workSession);
            return workSession;
        }
    }
}