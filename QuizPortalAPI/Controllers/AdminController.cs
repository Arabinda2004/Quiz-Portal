using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;
        private readonly IStudentResponseService _responseService;
        private readonly IExamService _examService;

        public AdminController(IUserService userService, ILogger<AdminController> logger, IExamService examService, IStudentResponseService responseService)
        {
            _userService = userService;
            _logger = logger;
            _examService = examService;
            _responseService = responseService;
        }

        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        /// <summary>
        /// GET: api/admin/users
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _logger.LogInformation("Admin retrieved all users successfully");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// GET: api/admin/users/{id}
        /// Get a specific user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Admin requested user with ID {id} not found");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin retrieved user {id} successfully");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// Get exam-wide statistics and summary
        /// GET /api/admin/exams/{examId}/responses/statistics
        /// </summary>
        [HttpGet("exams/{examId}/responses/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetExamStatistics(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var adminId = GetLoggedInUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var stats = await _responseService.GetExamStatisticsAsync(examId);

                _logger.LogInformation($"Admin {adminId} retrieved statistics for exam {examId}");
                return Ok(new { data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam statistics: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exam statistics" });
            }
        }


        /// <summary>
        /// Get exam details by ID (for admin)
        /// GET /api/admin/exams/{id}
        /// </summary>
        [HttpGet("exams/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetExamById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var adminId = GetLoggedInUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });
                
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                _logger.LogInformation($"Admin {adminId} retrieved exam {id}");
                return Ok(new { data = exam });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the exam" });
            }
        }
    }
}