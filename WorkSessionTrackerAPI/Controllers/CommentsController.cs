using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using WorkSessionTrackerAPI.DTOs;
using WorkSessionTrackerAPI.Authorization;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Models;
using System.Linq;

namespace WorkSessionTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All methods in this controller require authentication by default
    public class CommentsController : BaseApiController
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService; // Still needed for GetSupervisorWithEmployeesAsync
        private readonly UserManager<User> _userManager; // Inject UserManager for user-related checks
        private readonly IWorkSessionRepository _workSessionRepository; // Needed for work session existence and employeeId
        private readonly IAuthorizationService _authorizationService;

        public CommentsController(ICommentService commentService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository, IAuthorizationService authorizationService) : base()
        {
            _commentService = commentService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Authorize(Policy = Policies.CompanyOnly)]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            var workSession = await _workSessionRepository.GetByIdAsync(dto.WorkSessionId);
            if (workSession == null)
            {
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.CanCommentOnWorkSession);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You can only comment on work sessions of your own students.");
            }

            var comment = await _commentService.CreateCommentAsync(dto, authenticatedUserId);
            if (comment == null)
            {
                return BadRequest("Could not create comment. A comment might already exist for this work session.");
            }
            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment is null) return NotFound("Comment not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, Policies.CanViewComment);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to view this comment.");
            }

            return Ok(comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null) return NotFound("Comment not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingComment, Policies.IsCommentOwner);
            if (!authorizationResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to update this comment.");
            }

            var updatedComment = await _commentService.UpdateCommentAsync(existingComment, dto);
            if (updatedComment == null) return BadRequest("Failed to update comment.");
            return Ok(updatedComment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null) return NotFound("Comment not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingComment, Policies.IsCommentOwner);
            if (!authorizationResult.Succeeded) return Forbid("You are not authorized to delete this comment.");
            
            var result = await _commentService.DeleteCommentAsync(existingComment);
            if (!result) return BadRequest("Failed to delete comment.");
            return NoContent();
        }
    }
}
