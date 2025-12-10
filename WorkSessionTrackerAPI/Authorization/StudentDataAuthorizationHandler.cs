using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.Authorization
{
    public class StudentDataAuthorizationHandler : AuthorizationHandler<StudentDataRequirement, int>
    {
        private readonly ApplicationDbContext _context;

        public StudentDataAuthorizationHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentDataRequirement requirement, int studentId)
        {
            // Admins can access any student's data
            if (context.User.IsInRole(UserRoleEnum.Admin.ToString()))
            {
                context.Succeed(requirement);
                return;
            }

            var authenticatedUserId = int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // The student can access their own data
            if (authenticatedUserId == studentId)
            {
                context.Succeed(requirement);
                return;
            }

            // A company can access data of a student linked to them
            var isCompanyAndStudentIsTheirs = await _context.Users.AnyAsync(u => u.Id == studentId && ((Student)u).CompanyId == authenticatedUserId);
            if (context.User.IsInRole(UserRoleEnum.Company.ToString()) && isCompanyAndStudentIsTheirs)
            {
                context.Succeed(requirement);
            }
        }
    }
}