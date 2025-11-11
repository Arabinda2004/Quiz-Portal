using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/teacher/exams/{examId}/responses")]
    [Authorize(Roles = "Teacher")]
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

        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        /// <summary>
        /// Get all students who attempted an exam with their submission status
        /// GET /api/teacher/exams/{examId}/responses/students
        /// </summary>
        [HttpGet("students")]
        public async Task<IActionResult> GetStudentAttempts(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view attempts for exam {examId} they don't own");
                    return Forbid();
                }

                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                var attempts = await _responseService.GetAllStudentAttemptsAsync(examId);

                _logger.LogInformation($"Teacher {teacherId} retrieved student attempts for exam {examId}");
                return Ok(new
                {
                    examId = examId,
                    examName = exam.Title,
                    totalStudentsAttempted = attempts.TotalCount,
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
        public async Task<IActionResult> GetStudentResponses(int examId, int studentId)
        {
            try
            {

                var teacherId = GetLoggedInUserId()!;

                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view responses for exam {examId} they don't own");
                    return Forbid();
                }

                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

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
        /// Get exam-wide statistics and summary
        /// GET /api/teacher/exams/{examId}/responses/statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetExamStatistics(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view statistics for exam {examId} they don't own");
                    return Forbid();
                }

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