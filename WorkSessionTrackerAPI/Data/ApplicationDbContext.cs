using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Models; // Add this using statement

namespace WorkSessionTrackerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
    }
}
