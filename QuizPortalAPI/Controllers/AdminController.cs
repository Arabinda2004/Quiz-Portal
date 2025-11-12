using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.Services;

namespace QuizPortalAPI.Controllers
{
    [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// GET: api/admin/users
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet("users")]
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
        public async Task<IActionResult> GetExamStatistics(int examId)
        {
            try
            {   
                var stats = await _responseService.GetExamStatisticsAsync(examId);
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
        public async Task<IActionResult> GetExamById(int id)
        {
            try
            {
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                _logger.LogInformation($"Admin retrieved exam {id}");
                return Ok(new { data = exam });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the exam" });
            }
        }
        /// <summary>
        /// Get all exams
        /// GET /api/admin/exams/all
        /// </summary>
        [HttpGet("exams/all")]
        public async Task<IActionResult> GetAllExams()
        {
            try
            {
                var exams = await _examService.GetAllExamsAsync();

                _logger.LogInformation($"Admin retrieved all exams");
                return Ok(new { count = exams.Count(), data = exams });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all exams: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exams" });
            }
        }
    }
}