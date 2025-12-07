using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;

namespace WorkSessionTrackerAPI.Controllers
{
    /// <summary>
    /// Base controller for all API controllers to provide common functionality.
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token."));
        }

        protected string GetAuthenticatedUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException("User role not found in token.");
        }
    }
}
