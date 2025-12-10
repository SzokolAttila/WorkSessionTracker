using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Interfaces;

namespace WorkSessionTrackerAPI.Authorization
{
    public class StudentDataRequirement : IAuthorizationRequirement { }

    public class StudentDataAuthorizationHandler : AuthorizationHandler<StudentDataRequirement, int>
    {
        private readonly IUserService _userService;

        public StudentDataAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentDataRequirement requirement, int resourceStudentId)
        {
            var authenticatedUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!HasValidId(context, out var authenticatedUserId))
            {
                context.Fail();
                return;
            }

            // A student can access their own data
            if (context.User.IsInRole("Student") && authenticatedUserId == resourceStudentId)
            {
                context.Succeed(requirement);
                return;
            }

            // A company can access the data of their students
            if (context.User.IsInRole("Company"))
            {
                var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);
                if (company != null && company.Students.Any(s => s.Id == resourceStudentId))
                {
                    context.Succeed(requirement);
                }
            }
        }
        
        private bool HasValidId(AuthorizationHandlerContext context, out int authenticatedUserId)
            => int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out authenticatedUserId);
    }
}
