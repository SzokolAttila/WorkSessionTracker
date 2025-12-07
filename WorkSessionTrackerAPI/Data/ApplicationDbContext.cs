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
        public DbSet<WorkSession> WorkSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly configure TPH for the User hierarchy
            modelBuilder.Entity<User>()
                .ToTable("Users") // Ensure the single table for the hierarchy is named "Users"
                .HasDiscriminator<string>("UserType") // Define a discriminator column named "UserType"
                .HasValue<Employee>("Employee") // Map "Employee" string to Employee type
                .HasValue<Supervisor>("Supervisor"); // Map "Supervisor" string to Supervisor type
        }
    }
}
