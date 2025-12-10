using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;

namespace WorkSessionTrackerAPI.Authorization
{
    public class IsWorkSessionOwnerRequirement : IAuthorizationRequirement { }
    public class CanVerifyWorkSessionRequirement : IAuthorizationRequirement { }
    public class CanCommentOnWorkSessionRequirement : IAuthorizationRequirement { }

    public class WorkSessionAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IUserService _userService;

        public WorkSessionAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();
            if (context.Resource is not WorkSession resource)
            {
                return;
            }

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is IsWorkSessionOwnerRequirement ownerRequirement)
                {
                    await HandleRequirementAsync(context, ownerRequirement, resource);
                }
                else if (requirement is CanVerifyWorkSessionRequirement verifyRequirement)
                {
                    await HandleRequirementAsync(context, verifyRequirement, resource);
                }
                else if (requirement is CanCommentOnWorkSessionRequirement commentRequirement)
                {
                    await HandleRequirementAsync(context, commentRequirement, resource);
                }
            }
        }
        // Handles the "IsWorkSessionOwner" policy
        private Task HandleRequirementAsync(AuthorizationHandlerContext context, IsWorkSessionOwnerRequirement requirement, WorkSession resource)
        {
            if (HasValidId(context, out var authenticatedUserId))
            {
                if (context.User.IsInRole("Student") && resource.StudentId == authenticatedUserId)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }

        // Handles the "CanVerifyWorkSession" and "CanCommentOnWorkSession" policies
        private async Task HandleCompanyAccessRequirement(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, WorkSession resource)
        {
            if (!context.User.IsInRole("Company"))
            {
                return;
            }

            if (HasValidId(context, out var authenticatedUserId))
            {
                var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);

                if (company != null && company.Students.Any(s => s.Id == resource.StudentId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private Task HandleRequirementAsync(AuthorizationHandlerContext context, CanVerifyWorkSessionRequirement requirement, WorkSession resource)
        {
            return HandleCompanyAccessRequirement(context, requirement, resource);
        }

        private Task HandleRequirementAsync(AuthorizationHandlerContext context, CanCommentOnWorkSessionRequirement requirement, WorkSession resource)
        {
            return HandleCompanyAccessRequirement(context, requirement, resource);
        }
        
        private bool HasValidId(AuthorizationHandlerContext context, out int authenticatedUserId)
            => int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out authenticatedUserId);
    }
}
