using Microsoft.AspNetCore.Authorization;

namespace WorkSessionTrackerAPI.Authorization
{
    public class StudentDataRequirement : IAuthorizationRequirement { }
    public class IsWorkSessionOwnerRequirement : IAuthorizationRequirement { }
    public class CanVerifyWorkSessionRequirement : IAuthorizationRequirement { }
    public class CanCommentOnWorkSessionRequirement : IAuthorizationRequirement { }
    public class CanViewCommentRequirement : IAuthorizationRequirement { }
    public class IsCommentOwnerRequirement : IAuthorizationRequirement { }
}
