using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IWorkSessionRepository : IRepository<WorkSession>
    {
        Task<IEnumerable<WorkSession>> GetWorkSessionsByEmployeeIdAsync(int employeeId);
        Task<WorkSession?> GetWorkSessionByIdWithEmployeeAsync(int id); // For authorization checks
    }
}
