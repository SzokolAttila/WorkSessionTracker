using WorkSessionTrackerAPI.Models;
using System.Threading.Tasks;

namespace WorkSessionTrackerAPI.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<Comment?> GetCommentByWorkSessionIdAsync(int workSessionId);
    }
}
