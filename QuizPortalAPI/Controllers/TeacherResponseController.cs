using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.StudentResponse;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/teacher/exams/{examId}/responses")]
    [Authorize(Roles = "Teacher,Admin")]
    public class TeacherResponseController : ControllerBase
    {
        private readonly IStudentResponseService _responseService;
        private readonly IExamService _examService;
        private readonly ILogger<TeacherResponseController> _logger;

        public TeacherResponseController(
            IStudentResponseService responseService,
            IExamService examService,
            ILogger<TeacherResponseController> logger)
        {
            _responseService = responseService;
            _examService = examService;
            _logger = logger;
        }

        /// <summary>
        /// Get logged-in user's ID from JWT token
        /// </summary>
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return 0;
        }

        /// <summary>
        /// Get all students who attempted an exam with their submission status
        /// GET /api/teacher/exams/{examId}/responses/students
        /// </summary>
        [HttpGet("students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentAttempts(int examId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view attempts for exam {examId} they don't own");
                    return Forbid();
                }

                // ✅ Get exam details
                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                // ✅ Get all student attempts
                var attempts = await _responseService.GetAllStudentAttemptsAsync(examId, page, pageSize);

                _logger.LogInformation($"Teacher {teacherId} retrieved student attempts for exam {examId}");
                return Ok(new
                {
                    examId = examId,
                    examName = exam.Title,
                    totalStudentsAttempted = attempts.TotalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)attempts.TotalCount / pageSize),
                    data = attempts.Students
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student attempts: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving student attempts" });
            }
        }

        /// <summary>
        /// Get detailed responses of a specific student for an exam
        /// GET /api/teacher/exams/{examId}/responses/students/{studentId}
        /// </summary>
        [HttpGet("students/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentResponses(int examId, int studentId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || studentId <= 0)
                    return BadRequest(new { message = "Invalid exam or student ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view responses for exam {examId} they don't own");
                    return Forbid();
                }

                // ✅ Get exam details
                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                // ✅ Get student responses
                var responses = await _responseService.GetStudentExamResponsesAsync(examId, studentId);
                if (responses == null || responses.Responses.Count == 0)
                    return NotFound(new { message = "No responses found for this student" });

                _logger.LogInformation($"Teacher {teacherId} retrieved responses for student {studentId} in exam {examId}");
                return Ok(new
                {
                    examId = examId,
                    examName = exam.Title,
                    studentId = studentId,
                    totalQuestions = responses.TotalQuestions,
                    answeredQuestions = responses.AnsweredQuestions,
                    unansweredQuestions = responses.UnansweredQuestions,
                    progressPercentage = responses.TotalQuestions > 0
                        ? Math.Round(((decimal)responses.AnsweredQuestions / responses.TotalQuestions) * 100, 2)
                        : 0,
                    data = responses.Responses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving student responses" });
            }
        }

        /// <summary>
        /// Get all responses for a specific question (for grading)
        /// GET /api/teacher/exams/{examId}/responses/questions/{questionId}/all
        /// </summary>
        [HttpGet("questions/{questionId}/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQuestionResponses(int examId, int questionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view responses for exam {examId} they don't own");
                    return Forbid();
                }

                // ✅ Get responses for the question
                var responses = await _responseService.GetExamResponsesByQuestionPagedAsync(questionId, examId, page, pageSize);

                _logger.LogInformation($"Teacher {teacherId} retrieved responses for question {questionId}");
                return Ok(new
                {
                    examId = examId,
                    questionId = questionId,
                    totalResponses = responses.TotalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)responses.TotalCount / pageSize),
                    data = responses.Responses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving question responses" });
            }
        }

        /// <summary>
        /// Get question-wise statistics
        /// GET /api/teacher/exams/{examId}/responses/questions/{questionId}/statistics
        /// </summary>
        [HttpGet("questions/{questionId}/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQuestionStatistics(int examId, int questionId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view statistics for exam {examId} they don't own");
                    return Forbid();
                }

                // ✅ Get statistics
                var stats = await _responseService.GetQuestionStatisticsAsync(questionId, examId);

                _logger.LogInformation($"Teacher {teacherId} retrieved statistics for question {questionId}");
                return Ok(new { data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question statistics: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get exam-wide statistics and summary
        /// GET /api/teacher/exams/{examId}/responses/statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExamStatistics(int examId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view statistics for exam {examId} they don't own");
                    return Forbid();
                }

                // ✅ Get exam statistics
                var stats = await _responseService.GetExamStatisticsAsync(examId);

                _logger.LogInformation($"Teacher {teacherId} retrieved statistics for exam {examId}");
                return Ok(new { data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam statistics: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exam statistics" });
            }
        }
    }
}