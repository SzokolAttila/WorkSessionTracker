using WorkSessionTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IWorkSessionRepository : IRepository<WorkSession>
    {
        Task<IEnumerable<WorkSession>> GetWorkSessionsByStudentIdAsync(int studentId); // Renamed from GetWorkSessionsByEmployeeIdAsync
    }
}
