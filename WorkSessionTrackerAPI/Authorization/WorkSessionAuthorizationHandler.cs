using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.Authorization
{
    public class WorkSessionAuthorizationHandler :
        IAuthorizationHandler // Implement the root interface to handle multiple requirements
    {
        private readonly ApplicationDbContext _context;

        public WorkSessionAuthorizationHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            // Admins can do anything
            if (context.User.IsInRole(UserRoleEnum.Admin.ToString()))
            {
                foreach (var requirement in context.PendingRequirements)
                    context.Succeed(requirement);
                return;
            }

            var authenticatedUserId = int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var workSession = context.Resource as WorkSession;

            if (workSession == null) return;

            foreach (var requirement in context.PendingRequirements)
            {
                if (requirement is IsWorkSessionOwnerRequirement)
                {
                    if (workSession.StudentId == authenticatedUserId)
                    {
                        context.Succeed(requirement);
                    }
                }
                else if (requirement is CanVerifyWorkSessionRequirement)
                {
                    var isCompanyAndStudentIsTheirs = await _context.Users.AnyAsync(u => u.Id == workSession.StudentId && ((Student)u).CompanyId == authenticatedUserId);
                    if (context.User.IsInRole(UserRoleEnum.Company.ToString()) && isCompanyAndStudentIsTheirs)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}