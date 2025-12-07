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
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly configure TPH for the User hierarchy
            modelBuilder.Entity<User>()
                .ToTable("Users") // Ensure the single table for the hierarchy is named "Users"
                .HasDiscriminator<string>("UserType") // Define a discriminator column named "UserType"
                .HasValue<Employee>("Employee") // Map "Employee" string to Employee type
                .HasValue<Supervisor>("Supervisor"); // Map "Supervisor" string to Supervisor type

            // Configure WorkSession to Comment one-to-one relationship
            modelBuilder.Entity<WorkSession>()
                .HasOne(ws => ws.Comment)
                .WithOne(c => c.WorkSession)
                .HasForeignKey<Comment>(c => c.WorkSessionId) // Comment has FK to WorkSession
                .OnDelete(DeleteBehavior.Cascade); // When WorkSession is deleted, delete its Comment

            // Configure Supervisor to Comment one-to-many relationship
            modelBuilder.Entity<Supervisor>()
                .HasMany(s => s.Comments)
                .WithOne(c => c.Supervisor)
                .HasForeignKey(c => c.SupervisorId) // Comment has FK to Supervisor
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete when Supervisor is deleted
        }
    }
}
