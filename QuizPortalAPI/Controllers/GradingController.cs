using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Grading;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/teacher/grading")]
    [Authorize(Roles = "Teacher,Admin")]
    public class GradingController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        private readonly IExamService _examService;
        private readonly ILogger<GradingController> _logger;

        public GradingController(
            IGradingService gradingService,
            IExamService examService,
            ILogger<GradingController> logger)
        {
            _gradingService = gradingService;
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
        /// Get all pending responses for an exam
        /// GET /api/teacher/grading/exams/{examId}/pending
        /// </summary>
        [HttpGet("exams/{examId}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingResponses(int examId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var pendingResponses = await _gradingService.GetPendingResponsesAsync(examId, teacherId, page, pageSize);

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
        /// Get pending responses for a specific question
        /// GET /api/teacher/grading/exams/{examId}/questions/{questionId}/pending
        /// </summary>
        [HttpGet("exams/{examId}/questions/{questionId}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingResponsesByQuestion(int examId, int questionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var pendingResponses = await _gradingService.GetPendingResponsesByQuestionAsync(examId, questionId, teacherId, page, pageSize);

                _logger.LogInformation($"Teacher {teacherId} retrieved pending responses for question {questionId}");
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
                _logger.LogError($"Error retrieving pending responses by question: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving pending responses" });
            }
        }

        /// <summary>
        /// Get pending responses for a specific student
        /// GET /api/teacher/grading/exams/{examId}/students/{studentId}/pending
        /// </summary>
        [HttpGet("exams/{examId}/students/{studentId}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingResponsesByStudent(int examId, int studentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (examId <= 0 || studentId <= 0)
                    return BadRequest(new { message = "Invalid exam or student ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var pendingResponses = await _gradingService.GetPendingResponsesByStudentAsync(examId, studentId, teacherId, page, pageSize);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResponseForGrading(int responseId)
        {
            try
            {
                if (responseId <= 0)
                    return BadRequest(new { message = "Invalid response ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var response = await _gradingService.GetResponseForGradingAsync(responseId, teacherId);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GradeSingleResponse(int responseId, [FromBody] GradeSingleResponseDTO gradeDto)
        {
            try
            {
                if (responseId <= 0)
                    return BadRequest(new { message = "Invalid response ID" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var success = await _gradingService.GradeSingleResponseAsync(responseId, teacherId, gradeDto);

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
        /// Grade multiple responses in batch
        /// POST /api/teacher/grading/batch-grade
        /// </summary>
        [HttpPost("batch-grade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GradeBatchResponses([FromBody] BatchGradeDTO batchGradeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (batchGradeDto.Responses.Count == 0)
                    return BadRequest(new { message = "At least one response must be provided" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var success = await _gradingService.GradeBatchResponsesAsync(teacherId, batchGradeDto);

                _logger.LogInformation($"Teacher {teacherId} batch graded {batchGradeDto.Responses.Count} responses");
                return Ok(new
                {
                    success = true,
                    message = $"{batchGradeDto.Responses.Count} responses graded successfully"
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
                _logger.LogError($"Error batch grading responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while batch grading responses" });
            }
        }

        /// <summary>
        /// Get grading history for a response
        /// GET /api/teacher/grading/responses/{responseId}/history
        /// </summary>
        [HttpGet("responses/{responseId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGradingHistory(int responseId)
        {
            try
            {
                if (responseId <= 0)
                    return BadRequest(new { message = "Invalid response ID" });

                var gradingHistory = await _gradingService.GetGradingHistoryAsync(responseId);

                _logger.LogInformation($"Retrieved grading history for response {responseId}");
                return Ok(new
                {
                    success = true,
                    data = gradingHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving grading history: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving grading history" });
            }
        }

        /// <summary>
        /// Get grading statistics for an exam
        /// GET /api/teacher/grading/exams/{examId}/statistics
        /// </summary>
        [HttpGet("exams/{examId}/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGradingStatistics(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var stats = await _gradingService.GetGradingStatsAsync(examId, teacherId);

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
        /// Get all graded responses for a student
        /// GET /api/teacher/grading/exams/{examId}/students/{studentId}/graded-responses
        /// </summary>
        [HttpGet("exams/{examId}/students/{studentId}/graded-responses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentGradedResponses(int examId, int studentId)
        {
            try
            {
                if (examId <= 0 || studentId <= 0)
                    return BadRequest(new { message = "Invalid exam or student ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var gradedResponses = await _gradingService.GetStudentGradedResponsesAsync(examId, studentId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} retrieved graded responses for student {studentId}");
                return Ok(new
                {
                    success = true,
                    data = gradedResponses
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
                _logger.LogError($"Error retrieving student graded responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving graded responses" });
            }
        }

        /// <summary>
        /// Regrade a response
        /// POST /api/teacher/grading/responses/{responseId}/regrade
        /// </summary>
        [HttpPost("responses/{responseId}/regrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegradeResponse(int responseId, [FromBody] RegradingDTO regradingDto)
        {
            try
            {
                if (responseId <= 0)
                    return BadRequest(new { message = "Invalid response ID" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var success = await _gradingService.RegradeResponseAsync(responseId, teacherId, regradingDto);

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

        /// <summary>
        /// Mark a question as fully graded
        /// PUT /api/teacher/grading/exams/{examId}/questions/{questionId}/mark-graded
        /// </summary>
        [HttpPut("exams/{examId}/questions/{questionId}/mark-graded")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkQuestionGraded(int examId, int questionId)
        {
            try
            {
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var success = await _gradingService.MarkQuestionGradedAsync(examId, questionId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} marked question {questionId} as graded");
                return Ok(new
                {
                    success = true,
                    message = "Question marked as graded successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking question as graded: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while marking question as graded" });
            }
        }
    }
}
