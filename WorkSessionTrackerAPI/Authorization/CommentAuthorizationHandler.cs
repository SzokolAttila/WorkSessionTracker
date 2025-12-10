using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Models;
using WorkSessionTrackerAPI.Models.Enums;

namespace WorkSessionTrackerAPI.Authorization
{
    public class CommentAuthorizationHandler : IAuthorizationHandler
    {
        private readonly ApplicationDbContext _context;

        public CommentAuthorizationHandler(ApplicationDbContext context)
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

            var authenticatedUserId = int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // The resource can be a Comment or a WorkSession
            var workSession = context.Resource as WorkSession;
            var comment = context.Resource as Comment;

            // Get the studentId, whether from the comment's work session or the work session directly
            int? studentId = null;
            if (comment != null) studentId = (await _context.WorkSessions.FindAsync(comment.WorkSessionId))?.StudentId;
            if (workSession != null) studentId = workSession.StudentId;

            foreach (var requirement in context.PendingRequirements)
            {
                if (requirement is CanCommentOnWorkSessionRequirement && studentId.HasValue)
                {
                    var isCompanyAndStudentIsTheirs = await _context.Users.AnyAsync(u => u.Id == studentId.Value && ((Student)u).CompanyId == authenticatedUserId);
                    if (context.User.IsInRole(UserRoleEnum.Company.ToString()) && isCompanyAndStudentIsTheirs)
                    {
                        context.Succeed(requirement);
                    }
                }
                else if (requirement is IsCommentOwnerRequirement && comment != null)
                {
                    if (comment.CompanyId == authenticatedUserId)
                    {
                        context.Succeed(requirement);
                    }
                }
                else if (requirement is CanViewCommentRequirement && studentId.HasValue)
                {
                    var isOwner = comment?.CompanyId == authenticatedUserId;
                    var isStudentOfComment = studentId.Value == authenticatedUserId;
                    if (isOwner || isStudentOfComment) context.Succeed(requirement);
                }
            }
        }
    }
}