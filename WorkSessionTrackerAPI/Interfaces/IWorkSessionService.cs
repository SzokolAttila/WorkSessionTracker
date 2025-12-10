using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IWorkSessionService
    {
        Task<WorkSession?> CreateWorkSessionAsync(CreateWorkSessionDto dto, int studentId);
        Task<IEnumerable<WorkSession>> GetStudentWorkSessionsAsync(int studentId);
        Task<WorkSessionSummaryDto> GetWorkSessionSummaryAsync(int studentId, int year, int month);
        Task<WorkSession?> UpdateWorkSessionAsync(WorkSession workSession, UpdateWorkSessionDto dto);
        Task<bool> DeleteWorkSessionAsync(WorkSession workSession);
        Task<WorkSession?> VerifyWorkSessionAsync(WorkSession workSession);
    }
}
