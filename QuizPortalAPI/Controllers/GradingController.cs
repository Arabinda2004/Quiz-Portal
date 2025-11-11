using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Grading;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/teacher/grading")]
    [Authorize(Roles = "Teacher")]
    public class GradingController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        private readonly ILogger<GradingController> _logger;

        public GradingController(
            IGradingService gradingService,
            ILogger<GradingController> logger)
        {
            _gradingService = gradingService;
            _logger = logger;
        }

        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        /// <summary>
        /// Get all pending responses for an exam
        /// GET /api/teacher/grading/exams/{examId}/pending
        /// </summary>
        [HttpGet("exams/{examId}/pending")]
        public async Task<IActionResult> GetPendingResponses(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var pendingResponses = await _gradingService.GetPendingResponsesAsync(examId, teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved pending responses for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = pendingResponses
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized access to exam {examId}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving pending responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving pending responses" });
            }
        }

        

        /// <summary>
        /// Get pending responses for a specific student
        /// GET /api/teacher/grading/exams/{examId}/students/{studentId}/pending
        /// </summary>
        [HttpGet("exams/{examId}/students/{studentId}/pending")]
        public async Task<IActionResult> GetPendingResponsesByStudent(int examId, int studentId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var pendingResponses = await _gradingService.GetPendingResponsesByStudentAsync(examId, studentId, teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved pending responses for student {studentId}");
                return Ok(new
                {
                    success = true,
                    data = pendingResponses
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving pending responses by student: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving pending responses" });
            }
        }

        /// <summary>
        /// Get a response for grading
        /// GET /api/teacher/grading/responses/{responseId}
        /// </summary>
        [HttpGet("responses/{responseId}")]
        public async Task<IActionResult> GetResponseForGrading(int responseId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var response = await _gradingService.GetResponseForGradingAsync(responseId, teacherId.Value);
                if (response == null)
                    return NotFound(new { message = "Response not found" });

                _logger.LogInformation($"Teacher {teacherId} retrieved response {responseId} for grading");
                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving response for grading: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the response" });
            }
        }

        /// <summary>
        /// Grade a single response
        /// POST /api/teacher/grading/responses/{responseId}/grade
        /// </summary>
        [HttpPost("responses/{responseId}/grade")]
        public async Task<IActionResult> GradeSingleResponse(int responseId, [FromBody] GradeSingleResponseDTO gradeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;
                var success = await _gradingService.GradeSingleResponseAsync(responseId, teacherId.Value, gradeDto);

                _logger.LogInformation($"Teacher {teacherId} graded response {responseId} with {gradeDto.MarksObtained} marks");
                return Ok(new
                {
                    success = true,
                    message = "Response graded successfully"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error grading response: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while grading the response" });
            }
        }


        /// <summary>
        /// Get grading statistics for an exam
        /// GET /api/teacher/grading/exams/{examId}/statistics
        /// </summary>
        [HttpGet("exams/{examId}/statistics")]
        public async Task<IActionResult> GetGradingStatistics(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var stats = await _gradingService.GetGradingStatsAsync(examId, teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved grading statistics for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving grading statistics: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving grading statistics" });
            }
        }


        /// <summary>
        /// Regrade a response
        /// POST /api/teacher/grading/responses/{responseId}/regrade
        /// </summary>
        [HttpPost("responses/{responseId}/regrade")]
        public async Task<IActionResult> RegradeResponse(int responseId, [FromBody] RegradingDTO regradingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;

                var success = await _gradingService.RegradeResponseAsync(responseId, teacherId.Value, regradingDto);

                _logger.LogInformation($"Teacher {teacherId} regraded response {responseId}");
                return Ok(new
                {
                    success = true,
                    message = "Response regraded successfully"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error regrading response: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while regrading the response" });
            }
        }

    }
}
