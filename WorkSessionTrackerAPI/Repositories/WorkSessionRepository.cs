using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Repositories
{
    public class WorkSessionRepository : IWorkSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkSession?> GetByIdAsync(int id)
        {
            return await _context.WorkSessions.FindAsync(id);
        }

        public async Task<IEnumerable<WorkSession>> GetAllAsync()
        {
            return await _context.WorkSessions.ToListAsync();
        }

        public async Task AddAsync(WorkSession entity)
        {
            await _context.WorkSessions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkSession entity)
        {
            _context.WorkSessions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WorkSession entity)
        {
            _context.WorkSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkSession>> GetWorkSessionsByEmployeeIdAsync(int employeeId)
        {
            return await _context.WorkSessions
                .Where(ws => ws.EmployeeId == employeeId)
                .Include(ws => ws.Comment) // Auto-include the Comment for each WorkSession
                .ToListAsync();
        }
    }
}
