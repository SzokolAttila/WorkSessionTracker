using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IWorkSessionRepository
    {
        Task<WorkSession?> GetByIdAsync(int id);
        Task<IEnumerable<WorkSession>> GetStudentWorkSessionsAsync(int studentId);
        Task<WorkSessionSummaryDto> GetWorkSessionSummaryAsync(int studentId, int year, int month);
        Task CreateAsync(WorkSession workSession);
        Task UpdateAsync(WorkSession workSession);
        Task<bool> DeleteAsync(WorkSession workSession);
    }
}
