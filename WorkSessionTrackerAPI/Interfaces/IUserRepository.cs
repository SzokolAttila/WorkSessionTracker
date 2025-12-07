using WorkSessionTrackerAPI.Models;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Interfaces;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Supervisor?> GetSupervisorByIdAsync(int id);
    }
}
