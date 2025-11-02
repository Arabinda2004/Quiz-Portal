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
        private readonly IExamService _examService;
        private readonly IGradingService _gradingService;
        private readonly ILogger<ResultController> _logger;

        public ResultController(
            IResultService resultService,
            IExamService examService,
            IGradingService gradingService,
            ILogger<ResultController> logger)
        {
            _resultService = resultService;
            _examService = examService;
            _gradingService = gradingService;
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
        /// Get all exam attempts for logged-in student (including unpublished)
        /// GET /api/results/my-completed-exams
        /// Returns ALL completed exams, but masks sensitive data for unpublished results
        /// </summary>
        [HttpGet("my-completed-exams")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyCompletedExams([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 100;

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var results = await _resultService.GetStudentResultsAsync(studentId, page, pageSize);

                _logger.LogInformation($"Student {studentId} retrieved their completed exams - {results.Count} total");
                return Ok(new
                {
                    success = true,
                    page = page,
                    pageSize = pageSize,
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
        /// Get all exam attempts for logged-in student
        /// GET /api/results/my-results
        /// Only returns results that have been published by teacher (Status='Graded')
        /// </summary>
        [HttpGet("my-results")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyResults([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var results = await _resultService.GetStudentResultsAsync(studentId, page, pageSize);

                // ✅ SECURITY: Filter to only show published results (Status='Graded')
                results = results.Where(r => r.Status == "Graded").ToList();

                _logger.LogInformation($"Student {studentId} retrieved their exam results - {results.Count} published");
                return Ok(new
                {
                    success = true,
                    page = page,
                    pageSize = pageSize,
                    totalCount = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student results: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving your results" });
            }
        }

        /// <summary>
        /// Get specific exam result for logged-in student
        /// GET /api/results/exams/{examId}
        /// Only returns result if it has been published by teacher (Status='Graded')
        /// </summary>
        [HttpGet("exams/{examId}")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyExamResult(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var result = await _resultService.GetStudentExamResultAsync(examId, studentId);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                // ✅ SECURITY: Only allow student to view result if it's published (Status='Graded')
                if (result.Status != "Graded")
                {
                    _logger.LogWarning($"Student {studentId} attempted to view unpublished result for exam {examId}");
                    return Forbid();
                }

                _logger.LogInformation($"Student {studentId} retrieved result for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized access attempt to exam result");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam result: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the exam result" });
            }
        }

        /// <summary>
        /// Get result details with question-wise breakdown for logged-in student
        /// GET /api/results/exams/{examId}/details
        /// Only returns details if result has been published by teacher (Status='Graded')
        /// </summary>
        [HttpGet("exams/{examId}/details")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExamResultDetails(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ SECURITY: First check if result is published
                var result = await _resultService.GetStudentExamResultAsync(examId, studentId);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                if (result.Status != "Graded")
                {
                    _logger.LogWarning($"Student {studentId} attempted to view unpublished result details for exam {examId}");
                    return Forbid();
                }

                var resultDetails = await _resultService.GetExamResultDetailsAsync(examId, studentId);
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
        /// Get all results for an exam (Teacher/Admin only)
        /// GET /api/results/exams/{examId}/all-results
        /// </summary>
        [HttpGet("exams/{examId}/all-results")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExamAllResults(int examId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var results = await _resultService.GetExamAllResultsAsync(examId, teacherId, page, pageSize);

                _logger.LogInformation($"Teacher {teacherId} retrieved results for exam {examId}");
                return Ok(new
                {
                    success = true,
                    page = page,
                    pageSize = pageSize,
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
        /// Get result for specific student in an exam (Teacher/Admin only)
        /// GET /api/results/exams/{examId}/students/{studentId}
        /// </summary>
        [HttpGet("exams/{examId}/students/{studentId}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentExamResult(int examId, int studentId)
        {
            try
            {
                if (examId <= 0 || studentId <= 0)
                    return BadRequest(new { message = "Invalid exam or student ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var result = await _resultService.GetStudentExamResultForTeacherAsync(examId, studentId, teacherId);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                _logger.LogInformation($"Teacher {teacherId} retrieved result for student {studentId} in exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student exam result: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the result" });
            }
        }

        /// <summary>
        /// Get result statistics for an exam (Teacher/Admin only)
        /// GET /api/results/exams/{examId}/statistics
        /// </summary>
        [HttpGet("exams/{examId}/statistics")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResultStatistics(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var stats = await _resultService.GetResultStatisticsAsync(examId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} retrieved statistics for exam {examId}");
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
                _logger.LogError($"Error retrieving result statistics: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get result summary (grades count, pass/fail, etc.)
        /// GET /api/results/exams/{examId}/summary
        /// </summary>
        [HttpGet("exams/{examId}/summary")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResultSummary(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var summary = await _resultService.GetResultSummaryAsync(examId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} retrieved result summary for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = summary
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
                _logger.LogError($"Error retrieving result summary: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving result summary" });
            }
        }

        /// <summary>
        /// Get passed vs failed breakdown for an exam
        /// GET /api/results/exams/{examId}/pass-fail
        /// </summary>
        [HttpGet("exams/{examId}/pass-fail")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPassFailBreakdown(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var breakdown = await _resultService.GetPassFailBreakdownAsync(examId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} retrieved pass/fail breakdown for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = breakdown
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
                _logger.LogError($"Error retrieving pass/fail breakdown: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving pass/fail data" });
            }
        }

        /// <summary>
        /// Publish all results for an exam (Teacher/Admin only)
        /// POST /api/results/exams/{examId}/publish-results
        /// Marks all student results as "Graded" and makes them visible to students
        /// </summary>
        [HttpPost("exams/{examId}/publish-results")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PublishExamResults(int examId, [FromBody] SubmitResultDTO submitResultDto)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var result = await _resultService.PublishExamResultsAsync(
                    examId, 
                    teacherId, 
                    submitResultDto.PassingPercentage);

                _logger.LogInformation($"Teacher {teacherId} published results for exam {examId}");
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized access attempt to publish results for exam {examId}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return Conflict(new { message = ex.Message }); // 409 if already published
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing exam results: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while publishing results" });
            }
        }

        /// <summary>
        /// Publish exam - checks if all responses are graded first
        /// POST /api/results/exams/{examId}/publish
        /// Only available after all responses are manually graded
        /// </summary>
        [HttpPost("exams/{examId}/publish")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PublishExam(int examId, [FromBody] PublishExamRequestDTO request)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Validate passing percentage
                if (request.PassingPercentage < 0 || request.PassingPercentage > 100)
                    return BadRequest(new { message = "Passing percentage must be between 0 and 100" });

                var result = await _resultService.PublishExamAsync(
                    examId, 
                    teacherId, 
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
        /// Get grading progress for an exam
        /// GET /api/results/exams/{examId}/grading-progress
        /// </summary>
        [HttpGet("exams/{examId}/grading-progress")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGradingProgress(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var progress = await _resultService.GetGradingProgressAsync(examId, teacherId);

                _logger.LogInformation($"Teacher {teacherId} retrieved grading progress for exam {examId}");
                return Ok(new
                {
                    success = true,
                    data = progress
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
                _logger.LogError($"Error retrieving grading progress: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving grading progress" });
            }
        }

        /// <summary>
        /// Get exam publication status
        /// GET /api/results/exams/{examId}/publication-status
        /// </summary>
        [HttpGet("exams/{examId}/publication-status")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublicationStatus(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var status = await _resultService.GetExamPublicationStatusAsync(examId, teacherId);

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
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnpublishExam(int examId, [FromBody] UnpublishExamRequestDTO? request)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var result = await _resultService.UnpublishExamAsync(
                    examId, 
                    teacherId,
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublishedResults()
        {
            try
            {
                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var results = await _resultService.GetPublishedExamResultsAsync(studentId);

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

        /// <summary>
        /// Recalculate ranks for all students in an exam (Teacher/Admin only)
        /// POST /api/results/exams/{examId}/recalculate-ranks
        /// </summary>
        [HttpPost("exams/{examId}/recalculate-ranks")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecalculateExamRanks(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                await _resultService.RecalculateExamRanksAsync(examId);

                _logger.LogInformation($"Teacher {teacherId} recalculated ranks for exam {examId}");
                return Ok(new
                {
                    success = true,
                    message = "Ranks recalculated successfully for all students in the exam"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recalculating ranks for exam {examId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while recalculating ranks" });
            }
        }
    }
}
