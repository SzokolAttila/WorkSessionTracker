using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Models; // Add this using statement

namespace WorkSessionTrackerAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WorkSession> WorkSessions { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Identity Roles using EF Core's built-in data seeding.
            // This is the recommended approach for static, essential data.
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Student", NormalizedName = "STUDENT", ConcurrencyStamp = "50a2fb83-cdb6-446a-b2de-16125ef39220" },
                new IdentityRole<int> { Id = 2, Name = "Company", NormalizedName = "COMPANY", ConcurrencyStamp = "e26fa680-19d8-4c1f-8461-b4f133b42fd6" },
                new IdentityRole<int> { Id = 3, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "3a7df718-76cd-426e-bfdd-785bd9d16e7b" }
            );

            // Explicitly configure TPH for the User hierarchy
            modelBuilder.Entity<User>()
                .ToTable("Users") // Ensure the single table for the hierarchy is named "Users"
                .HasDiscriminator<string>("UserType") // Define a discriminator column named "UserType"
                .HasValue<Student>("Student") // Map "Student" string to Student type
                .HasValue<Company>("Company"); // Map "Company" string to Company type

            // Explicitly set the max length for the discriminator column to prevent truncation errors.
            // A length of 50 is a safe value for "Student", "Company", or any future roles.
            modelBuilder.Entity<User>().Property("UserType")
                .HasMaxLength(50);

            // Configure WorkSession to Comment one-to-one relationship
            modelBuilder.Entity<WorkSession>()
                .HasOne(ws => ws.Comment)
                .WithOne(c => c.WorkSession)
                .HasForeignKey<Comment>(c => c.WorkSessionId) // Comment has FK to WorkSession
                .OnDelete(DeleteBehavior.Cascade); // When WorkSession is deleted, delete its Comment

            // Configure Company to Comment one-to-many relationship
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Comments)
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId) // Comment has FK to Company
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete when Supervisor is deleted
        }
    }
}
