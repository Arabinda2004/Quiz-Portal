using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Result;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize]
    public class ResultController : ControllerBase
    {
        private readonly IResultService _resultService;
        private readonly ILogger<ResultController> _logger;

        public ResultController(
            IResultService resultService,
            ILogger<ResultController> logger)
        {
            _resultService = resultService;
            _logger = logger;
        }

        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        /// <summary>
        /// GET /api/results/my-completed-exams
        /// Returns ALL completed exams
        /// </summary>
        [HttpGet("my-completed-exams")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCompletedExams()
        {
            try
            {
                var studentId = GetLoggedInUserId()!;

                var results = await _resultService.GetStudentResultsAsync(studentId.Value);

                _logger.LogInformation($"Student {studentId} retrieved their completed exams - {results.Count} total");
                return Ok(new
                {
                    success = true,
                    totalCount = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving completed exams: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving your completed exams" });
            }
        }

        /// <summary>
        /// Get result details with question-wise breakdown for logged-in student
        /// GET /api/results/exams/{examId}/details
        /// Only returns details if result has been published by teacher
        /// </summary>
        [HttpGet("exams/{examId}/details")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExamResultDetails(int examId)
        {
            try
            {
                var studentId = GetLoggedInUserId()!;

                var result = await _resultService.GetStudentExamResultAsync(examId, studentId.Value);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                if (result.Status != "Graded")
                {
                    _logger.LogWarning($"Student {studentId} attempted to view unpublished result details for exam {examId}");
                    return Forbid();
                }

                var resultDetails = await _resultService.GetExamResultDetailsAsync(examId, studentId.Value);
                if (resultDetails == null)
                    return NotFound(new { message = "Result not found" });

                _logger.LogInformation($"Student {studentId} retrieved detailed result for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = resultDetails
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving result details: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving result details" });
            }
        }

        /// <summary>
        /// Get all results for an exam (Teacher only)
        /// GET /api/results/exams/{examId}/all-results
        /// </summary>
        [HttpGet("exams/{examId}/all-results")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetExamAllResults(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var results = await _resultService.GetExamAllResultsAsync(examId, teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved results for exam {examId}");
                return Ok(new
                {
                    success = true,
                    totalCount = results.Count,
                    data = results
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
                _logger.LogError($"Error retrieving exam results: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exam results" });
            }
        }

        /// <summary>
        /// POST /api/results/exams/{examId}/publish
        /// Only available after all responses are manually graded
        /// </summary>
        [HttpPost("exams/{examId}/publish")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> PublishExam(int examId, [FromBody] PublishExamRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;

                if (request.PassingPercentage < 0 || request.PassingPercentage > 100)
                    return BadRequest(new { message = "Passing percentage must be between 0 and 100" });

                var result = await _resultService.PublishExamAsync(
                    examId, 
                    teacherId.Value, 
                    request.PassingPercentage,
                    request.PublicationNotes);

                _logger.LogInformation($"Teacher {teacherId} published exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized access attempt to publish exam {examId}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing exam {examId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while publishing the exam" });
            }
        }

        /// <summary>
        /// Get exam publication status
        /// GET /api/results/exams/{examId}/publication-status
        /// </summary>
        [HttpGet("exams/{examId}/publication-status")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetPublicationStatus(int examId)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var status = await _resultService.GetExamPublicationStatusAsync(examId, teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved publication status for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = status
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
                _logger.LogError($"Error retrieving publication status: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving publication status" });
            }
        }

        /// <summary>
        /// Unpublish an exam (reverses publication)
        /// POST /api/results/exams/{examId}/unpublish
        /// </summary>
        [HttpPost("exams/{examId}/unpublish")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UnpublishExam(int examId, [FromBody] UnpublishExamRequestDTO? request)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                var result = await _resultService.UnpublishExamAsync(
                    examId, 
                    teacherId.Value,
                    request?.Reason);

                _logger.LogInformation($"Teacher {teacherId} unpublished exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized access attempt to unpublish exam {examId}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unpublishing exam {examId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while unpublishing the exam" });
            }
        }

        /// <summary>
        /// Get all published results for logged-in student
        /// GET /api/results/published
        /// Only returns published results
        /// </summary>
        [HttpGet("published")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetPublishedResults()
        {
            try
            {
                var studentId = GetLoggedInUserId()!;

                var results = await _resultService.GetPublishedExamResultsAsync(studentId.Value);

                _logger.LogInformation($"Student {studentId} retrieved published exam results - {results.Count} total");
                return Ok(new
                {
                    success = true,
                    totalCount = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving published results: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving your published results" });
            }
        }
    }
}
