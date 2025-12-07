using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using WorkSessionTrackerAPI.Interfaces;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Set<User>().FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Set<User>().ToListAsync();
        }

        public async Task AddAsync(User entity)
        {
            await _context.Set<User>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            _context.Set<User>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User entity)
        {
            _context.Set<User>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Set<User>().OfType<Employee>().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Supervisor?> GetSupervisorByIdAsync(int id)
        {
            return await _context.Set<User>().OfType<Supervisor>().FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
