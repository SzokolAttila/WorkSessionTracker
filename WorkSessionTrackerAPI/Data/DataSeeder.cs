using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.Data
{
    public static class DataSeeder
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            // Seed Roles
            string[] roleNames = { UserRoleEnum.Admin.ToString(), UserRoleEnum.Student.ToString(), UserRoleEnum.Company.ToString() };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    logger.LogInformation("Role '{RoleName}' created.", roleName);
                }
            }

            // Seed Admin User
            var adminEmail = configuration["AdminUser:Email"];
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = configuration["AdminUser:DisplayName"],
                    EmailConfirmed = true // Admin user is confirmed by default
                };

                var createResult = await userManager.CreateAsync(newAdminUser, configuration["AdminUser:Password"]);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, UserRoleEnum.Admin.ToString());
                    logger.LogInformation("Admin user created successfully.");
                }
            }
        }
    }
}