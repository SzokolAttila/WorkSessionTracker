using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Moq;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Interfaces;

namespace WorkSessionTrackerAPI.IntegrationTests
{
    // It's conventional to use a generic placeholder like 'TStartup' instead of a concrete class name.
    // This makes the factory more reusable and less confusing.
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set the environment to "Testing" to prevent production/development services
            // like the DataSeeder from running.
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // This is the crucial step to prevent the "multiple database providers" error.
                // We find the original DbContextOptions registration and remove it from the services collection.
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                
                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(System.Data.Common.DbConnection));

                if (dbConnectionDescriptor != null)
                {
                    services.Remove(dbConnectionDescriptor);
                }

                // Add a new DbContext that uses an in-memory database for testing.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Find and remove the real IEmailService registration.
                var emailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailServiceDescriptor != null)
                {
                    services.Remove(emailServiceDescriptor);
                }

                // Add a mock IEmailService that does nothing, so we don't send real emails.
                var mockEmailService = new Mock<IEmailService>();
                mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(mockEmailService.Object);
            });
        }
    }
}
