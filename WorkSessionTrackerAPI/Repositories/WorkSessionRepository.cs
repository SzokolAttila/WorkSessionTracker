using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Repositories
{
    public class WorkSessionRepository : IWorkSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(WorkSession workSession)
        {
            await _context.WorkSessions.AddAsync(workSession);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(WorkSession workSession)
        {
            _context.WorkSessions.Remove(workSession);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<WorkSession?> GetByIdAsync(int id)
        {
            return await _context.WorkSessions.FindAsync(id);
        }

        public async Task<IEnumerable<WorkSession>> GetStudentWorkSessionsAsync(int studentId)
        {
            return await _context.WorkSessions
                .AsNoTracking()
                .Where(ws => ws.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<WorkSessionSummaryDto> GetWorkSessionSummaryAsync(int studentId, int year, int month)
        {
            var sessionsInMonth = _context.WorkSessions
                .AsNoTracking()
                .Where(ws => ws.StudentId == studentId &&
                             ws.StartDateTime.Year == year &&
                             ws.StartDateTime.Month == month);

            var totalHours = await sessionsInMonth
                .SumAsync(ws => (ws.EndDateTime - ws.StartDateTime).TotalHours);

            var verifiedHours = await sessionsInMonth
                .Where(ws => ws.Verified)
                .SumAsync(ws => (ws.EndDateTime - ws.StartDateTime).TotalHours);

            return new WorkSessionSummaryDto
            {
                TotalHours = totalHours,
                VerifiedHours = verifiedHours
            };
        }

        public async Task UpdateAsync(WorkSession workSession)
        {
            _context.WorkSessions.Update(workSession);
            await _context.SaveChangesAsync();
        }
    }
}