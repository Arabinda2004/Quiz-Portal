using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.StudentResponse;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/exams/{examId}/responses")]
    [Authorize(Roles = "Student")]
    public class StudentResponseController : ControllerBase
    {
        private readonly IStudentResponseService _responseService;
        private readonly IExamService _examService;
        private readonly ILogger<StudentResponseController> _logger;

        public StudentResponseController(
            IStudentResponseService responseService,
            IExamService examService,
            ILogger<StudentResponseController> logger)
        {
            _responseService = responseService;
            _examService = examService;
            _logger = logger;
        }

        /// <summary>
        /// Get logged-in user's ID from JWT token
        /// </summary>
        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        /// <summary>
        /// Student submits an answer to a question
        /// POST /api/exams/{examId}/responses
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int examId, [FromBody] CreateStudentResponseDTO createResponseDTO)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (createResponseDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var canSubmit = await _responseService.CanSubmitAnswerAsync(examId, studentId.Value);
                if (!canSubmit)
                {
                    _logger.LogWarning($"Student {studentId} attempted to submit answer when exam is not active or already published");
                    return Conflict(new { message = "Exam is not active or results have already been published. Submission not allowed" });
                }

                // Submit answer
                var submittedResponse = await _responseService.SubmitAnswerAsync(examId, studentId.Value, createResponseDTO);

                _logger.LogInformation($"Student {studentId} submitted answer for exam {examId}");
                return CreatedAtAction(nameof(GetResponseById), 
                    new { examId, id = submittedResponse.ResponseID },
                    new { message = "Answer submitted successfully", data = submittedResponse });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting answer: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while submitting your answer" });
            }
        }

        /// <summary>
        /// Get all responses submitted by student for an exam
        /// GET /api/exams/{examId}/responses
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStudentExamResponses(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var examResponses = await _responseService.GetStudentExamResponsesAsync(examId, studentId.Value);

                _logger.LogInformation($"Student {studentId} retrieved responses for exam {examId}");
                return Ok(new
                {
                    examID = examResponses.ExamID,
                    examName = examResponses.ExamName,
                    totalQuestions = examResponses.TotalQuestions,
                    answeredQuestions = examResponses.AnsweredQuestions,
                    unansweredQuestions = examResponses.UnansweredQuestions,
                    progressPercentage = examResponses.TotalQuestions > 0 
                        ? Math.Round(((decimal)examResponses.AnsweredQuestions / examResponses.TotalQuestions) * 100, 2)
                        : 0,
                    data = examResponses.Responses
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam responses: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving your responses" });
            }
        }


        /// <summary>
        /// Update/Re-submit an answer to a question
        /// PUT /api/exams/{examId}/responses/{questionId}
        /// </summary>
        [HttpPut("{questionId}")]
        public async Task<IActionResult> UpdateAnswer(int examId, int questionId, [FromBody] CreateStudentResponseDTO updateResponseDTO)
        {
            try
            {
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                if (updateResponseDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var canSubmit = await _responseService.CanSubmitAnswerAsync(examId, studentId.Value);
                if (!canSubmit)
                {
                    _logger.LogWarning($"Student {studentId} attempted to update answer when exam is not active or already published");
                    return Conflict(new { message = "Exam is not active or results have already been published. Cannot update answer" });
                }

                // Check if response exists for this question
                var responseExists = await _responseService.ResponseExistsAsync(examId, questionId, studentId.Value);
                if (!responseExists)
                {
                    _logger.LogWarning($"No response found for student {studentId} to question {questionId}");
                    return NotFound(new { message = "Response not found. Please submit an answer first" });
                }

                // Update answer
                updateResponseDTO.QuestionID = questionId;
                var updatedResponse = await _responseService.SubmitAnswerAsync(examId, studentId.Value, updateResponseDTO);

                _logger.LogInformation($"Student {studentId} updated answer for question {questionId} in exam {examId}");
                return Ok(new { message = "Answer updated successfully", data = updatedResponse });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating answer: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating your answer" });
            }
        }

        /// <summary>
        /// Get student's response count for an exam
        /// GET /api/exams/{examId}/responses/count
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetResponseCount(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                // Get count
                var count = await _responseService.GetStudentResponseCountAsync(examId, studentId.Value);

                _logger.LogInformation($"Retrieved response count for student {studentId} in exam {examId}");
                return Ok(new { examID = examId, responseCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting response count: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving response count" });
            }
        }

        /// <summary>
        /// Get student's exam submission status
        /// GET /api/exams/{examId}/responses/status
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetSubmissionStatus(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                var responses = await _responseService.GetStudentExamResponsesAsync(examId, studentId.Value);

                var now = DateTime.UtcNow;
                var examStatus = now < exam.ScheduleStart ? "Not Started" :
                                 now > exam.ScheduleEnd ? "Ended" : "Active";

                var status = new
                {
                    examID = examId,
                    examName = exam.Title,
                    examStatus = examStatus,
                    scheduleStart = exam.ScheduleStart,
                    scheduleEnd = exam.ScheduleEnd,
                    durationMinutes = exam.DurationMinutes,
                    totalQuestions = responses.TotalQuestions,
                    answeredQuestions = responses.AnsweredQuestions,
                    unansweredQuestions = responses.UnansweredQuestions,
                    progressPercentage = responses.TotalQuestions > 0 
                        ? Math.Round(((decimal)responses.AnsweredQuestions / responses.TotalQuestions) * 100, 2)
                        : 0,
                    canSubmit = now >= exam.ScheduleStart && now <= exam.ScheduleEnd
                };

                _logger.LogInformation($"Retrieved submission status for student {studentId} in exam {examId}");
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting submission status: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving submission status" });
            }
        }

        /// <summary>
        /// Get a specific response by ID
        /// GET /api/exams/{examId}/responses/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResponseById(int examId, int id)
        {
            try
            {
                if (examId <= 0 || id <= 0)
                    return BadRequest(new { message = "Invalid exam or response ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var response = await _responseService.GetResponseByIdAsync(id);
                if (response == null)
                {
                    _logger.LogWarning($"Response {id} not found");
                    return NotFound(new { message = "Response not found" });
                }

                if (response.ExamID != examId)
                {
                    _logger.LogWarning($"Response {id} does not belong to exam {examId}");
                    return NotFound(new { message = "Response not found in this exam" });
                }

                if (response.StudentID != studentId)
                {
                    _logger.LogWarning($"Student {studentId} attempted to access response {id} they don't own");
                    return Forbid();
                }

                _logger.LogInformation($"Student {studentId} retrieved response {id}");
                return Ok(new { data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving response {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the response" });
            }
        }

        /// <summary>
        /// Finalize exam submission - creates a Result record to mark exam as completed
        /// POST /api/exams/{examId}/responses/submit
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> FinalizeExamSubmission(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                var result = await _responseService.FinalizeExamSubmissionAsync(examId, studentId.Value);
                
                _logger.LogInformation($"Student {studentId} finalized submission for exam {examId}");
                return Ok(new { 
                    message = "Exam submitted successfully", 
                    data = new {
                        resultID = result.ResultID,
                        examID = result.ExamID,
                        status = result.Status,
                        submittedAt = DateTime.UtcNow
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid submission attempt: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finalizing exam submission: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while finalizing your exam submission" });
            }
        }
    }
}