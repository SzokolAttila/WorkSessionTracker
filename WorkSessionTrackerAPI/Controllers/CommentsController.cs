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
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, IUserService userService, UserManager<User> userManager, IWorkSessionRepository workSessionRepository, IAuthorizationService authorizationService, ILogger<CommentsController> logger) : base()
        {
            _commentService = commentService;
            _userService = userService;
            _userManager = userManager;
            _workSessionRepository = workSessionRepository;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = Policies.CompanyOnly)]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to create a comment on WorkSession {WorkSessionId}", authenticatedUserId, dto.WorkSessionId);

            var workSession = await _workSessionRepository.GetByIdAsync(dto.WorkSessionId);
            if (workSession == null)
            {
                _logger.LogWarning("User {UserId} failed to create comment. WorkSession {WorkSessionId} not found.", authenticatedUserId, dto.WorkSessionId);
                return NotFound("Work session not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, workSession, Policies.CanCommentOnWorkSession);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to comment on WorkSession {WorkSessionId}.", authenticatedUserId, dto.WorkSessionId);
                return StatusCode(StatusCodes.Status403Forbidden, "You can only comment on work sessions of your own students.");
            }

            var comment = await _commentService.CreateCommentAsync(dto, authenticatedUserId);
            if (comment == null)
            {
                _logger.LogWarning("Comment creation failed for User {UserId} on WorkSession {WorkSessionId}. Service returned null.", authenticatedUserId, dto.WorkSessionId);
                return BadRequest("Could not create comment. A comment might already exist for this work session.");
            }
            _logger.LogInformation("User {UserId} successfully created Comment {CommentId} on WorkSession {WorkSessionId}", authenticatedUserId, comment.Id, dto.WorkSessionId);
            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to get comment with ID {CommentId}", authenticatedUserId, id);

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment is null)
            {
                _logger.LogWarning("User {UserId} failed to get comment. Comment {CommentId} not found.", authenticatedUserId, id);
                return NotFound("Comment not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, comment, Policies.CanViewComment);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to view Comment {CommentId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to view this comment.");
            }

            _logger.LogInformation("User {UserId} successfully retrieved Comment {CommentId}", authenticatedUserId, id);
            return Ok(comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to update comment with ID {CommentId}", authenticatedUserId, id);

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null)
            {
                _logger.LogWarning("User {UserId} failed to update comment. Comment {CommentId} not found.", authenticatedUserId, id);
                return NotFound("Comment not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingComment, Policies.IsCommentOwner);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to update Comment {CommentId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to update this comment.");
            }

            var updatedComment = await _commentService.UpdateCommentAsync(existingComment, dto);
            if (updatedComment == null)
            {
                _logger.LogError("Failed to update Comment {CommentId} for User {UserId} even after authorization.", id, authenticatedUserId);
                return BadRequest("Failed to update comment.");
            }
            _logger.LogInformation("User {UserId} successfully updated Comment {CommentId}", authenticatedUserId, id);
            return Ok(updatedComment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            _logger.LogInformation("User {UserId} attempting to delete comment with ID {CommentId}", authenticatedUserId, id);

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment is null)
            {
                _logger.LogWarning("User {UserId} failed to delete comment. Comment {CommentId} not found.", authenticatedUserId, id);
                return NotFound("Comment not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingComment, Policies.IsCommentOwner);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete Comment {CommentId}.", authenticatedUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to delete this comment.");
            }
            
            var result = await _commentService.DeleteCommentAsync(existingComment);
            if (!result)
            {
                _logger.LogError("Failed to delete Comment {CommentId} for User {UserId} even after authorization.", id, authenticatedUserId);
                return BadRequest("Failed to delete comment.");
            }
            _logger.LogInformation("User {UserId} successfully deleted Comment {CommentId}", authenticatedUserId, id);
            return NoContent();
        }
    }
}
