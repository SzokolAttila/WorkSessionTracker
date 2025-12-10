using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Authorization;

namespace WorkSessionTrackerAPI.Authorization
{
    public class CanViewCommentRequirement : IAuthorizationRequirement { }
    public class IsCommentOwnerRequirement : IAuthorizationRequirement { }

    public class CommentAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IUserService _userService;

        public CommentAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();
            if (context.Resource is not Comment resource)
            {
                return;
            }

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is CanViewCommentRequirement viewRequirement)
                {
                    await HandleRequirementAsync(context, viewRequirement, resource);
                }
                else if (requirement is IsCommentOwnerRequirement ownerRequirement)
                {
                    // This method returns a Task, so it should be awaited.
                    await HandleRequirementAsync(context, ownerRequirement, resource);
                }
            }
        }
        private async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewCommentRequirement requirement, Comment resource)
        {
            if (!HasValidId(context, out var authenticatedUserId)) return;

            // The student who owns the work session can view the comment
            if (context.User.IsInRole("Student") && resource.WorkSession?.StudentId == authenticatedUserId)
            {
                context.Succeed(requirement);
                return;
            }

            // The company that wrote the comment can view it
            if (context.User.IsInRole("Company") && resource.CompanyId == authenticatedUserId)
            {
                context.Succeed(requirement);
                return;
            }

            // A company can view comments on their students' work sessions
            if (context.User.IsInRole("Company"))
            {
                var company = await _userService.GetCompanyWithStudentsAsync(authenticatedUserId);
                if (company != null && company.Students.Any(s => s.Id == resource.WorkSession?.StudentId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private Task HandleRequirementAsync(AuthorizationHandlerContext context, IsCommentOwnerRequirement requirement, Comment resource)
        {
            if (HasValidId(context, out var authenticatedUserId))
            {
                if (context.User.IsInRole("Company") && resource.CompanyId == authenticatedUserId)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }

        private bool HasValidId(AuthorizationHandlerContext context, out int authenticatedUserId)
            => int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out authenticatedUserId);
    }
}
