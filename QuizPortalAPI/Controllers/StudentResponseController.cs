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
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return 0;
        }

        /// <summary>
        /// Student submits an answer to a question
        /// POST /api/exams/{examId}/responses
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitAnswer(int examId, [FromBody] CreateStudentResponseDTO createResponseDTO)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (createResponseDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Check if exam is active and not published
                var canSubmit = await _responseService.CanSubmitAnswerAsync(examId, studentId);
                if (!canSubmit)
                {
                    _logger.LogWarning($"Student {studentId} attempted to submit answer when exam is not active or already published");
                    return Conflict(new { message = "Exam is not active or results have already been published. Submission not allowed" });
                }

                // ✅ Submit answer
                var submittedResponse = await _responseService.SubmitAnswerAsync(examId, studentId, createResponseDTO);

                _logger.LogInformation($"Student {studentId} submitted answer for exam {examId}");
                return CreatedAtAction(nameof(GetResponseById), 
                    new { examId, id = submittedResponse.ResponseID },
                    new { message = "Answer submitted successfully", data = submittedResponse });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentExamResponses(int examId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get exam responses
                var examResponses = await _responseService.GetStudentExamResponsesAsync(examId, studentId);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAnswer(int examId, int questionId, [FromBody] CreateStudentResponseDTO updateResponseDTO)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                if (updateResponseDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Check if exam is active and not published
                var canSubmit = await _responseService.CanSubmitAnswerAsync(examId, studentId);
                if (!canSubmit)
                {
                    _logger.LogWarning($"Student {studentId} attempted to update answer when exam is not active or already published");
                    return Conflict(new { message = "Exam is not active or results have already been published. Cannot update answer" });
                }

                // ✅ Check if response exists for this question
                var responseExists = await _responseService.ResponseExistsAsync(examId, questionId, studentId);
                if (!responseExists)
                {
                    _logger.LogWarning($"No response found for student {studentId} to question {questionId}");
                    return NotFound(new { message = "Response not found. Please submit an answer first" });
                }

                // ✅ Update answer
                updateResponseDTO.QuestionID = questionId;
                var updatedResponse = await _responseService.SubmitAnswerAsync(examId, studentId, updateResponseDTO);

                _logger.LogInformation($"Student {studentId} updated answer for question {questionId} in exam {examId}");
                return Ok(new { message = "Answer updated successfully", data = updatedResponse });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
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
        /// Withdraw a submitted response (only during active exam)
        /// DELETE /api/exams/{examId}/responses/{questionId}
        /// </summary>
        [HttpDelete("{questionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WithdrawResponse(int examId, int questionId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || questionId <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Check if exam is active and not published
                var canSubmit = await _responseService.CanSubmitAnswerAsync(examId, studentId);
                if (!canSubmit)
                {
                    _logger.LogWarning($"Student {studentId} attempted to withdraw answer when exam is not active or already published");
                    return Conflict(new { message = "Exam is not active or results have already been published. Cannot withdraw response" });
                }

                // ✅ Find response
                var response = await _responseService.GetStudentExamResponsesAsync(examId, studentId);
                var responseToWithdraw = response.Responses
                    .FirstOrDefault(r => r.QuestionID == questionId);

                if (responseToWithdraw == null)
                {
                    _logger.LogWarning($"Response not found for withdrawal");
                    return NotFound(new { message = "Response not found" });
                }

                // ✅ Withdraw response
                var result = await _responseService.WithdrawResponseAsync(responseToWithdraw.ResponseID, studentId);
                if (!result)
                    return NotFound(new { message = "Response not found" });

                _logger.LogInformation($"Student {studentId} withdrew response for question {questionId} in exam {examId}");
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error withdrawing response: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while withdrawing your response" });
            }
        }

        /// <summary>
        /// Get student's response count for an exam
        /// GET /api/exams/{examId}/responses/count
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResponseCount(int examId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get count
                var count = await _responseService.GetStudentResponseCountAsync(examId, studentId);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubmissionStatus(int examId)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get exam details
                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                // ✅ Get responses
                var responses = await _responseService.GetStudentExamResponsesAsync(examId, studentId);

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

        // !! Commented out
        // /// <summary>
        // /// Get question-wise statistics (Teacher only - requires elevated permissions)
        // /// GET /api/exams/{examId}/responses/questions/{questionId}/statistics
        // /// </summary>
        // [HttpGet("questions/{questionId}/statistics")]
        // [Authorize(Roles = "Teacher,Admin")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> GetQuestionStatistics(int examId, int questionId)
        // {
        //     try
        //     {
        //         // ✅ Validate input
        //         if (examId <= 0 || questionId <= 0)
        //             return BadRequest(new { message = "Invalid exam or question ID" });

        //         var teacherId = GetLoggedInUserId();
        //         if (teacherId == 0)
        //             return Unauthorized(new { message = "Invalid user ID" });

        //         // ✅ Verify teacher owns the exam
        //         var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
        //         if (!isOwner)
        //         {
        //             _logger.LogWarning($"Teacher {teacherId} attempted to view statistics for exam {examId} they don't own");
        //             return Forbid();
        //         }

        //         // ✅ Get statistics
        //         var stats = await _responseService.GetQuestionStatisticsAsync(questionId);

        //         _logger.LogInformation($"Teacher {teacherId} retrieved statistics for question {questionId} in exam {examId}");
        //         return Ok(new { data = stats });
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         _logger.LogWarning($"Invalid operation: {ex.Message}");
        //         return NotFound(new { message = ex.Message });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"Error retrieving question statistics: {ex.Message}");
        //         return StatusCode(StatusCodes.Status500InternalServerError,
        //             new { message = "An error occurred while retrieving statistics" });
        //     }
        // }

        // /// <summary>
        // /// Get all responses for a question (Teacher only)
        // /// GET /api/exams/{examId}/responses/questions/{questionId}/all
        // /// </summary>
        // [HttpGet("questions/{questionId}/all")]
        // [Authorize(Roles = "Teacher,Admin")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> GetQuestionResponses(int examId, int questionId)
        // {
        //     try
        //     {
        //         // ✅ Validate input
        //         if (examId <= 0 || questionId <= 0)
        //             return BadRequest(new { message = "Invalid exam or question ID" });

        //         var teacherId = GetLoggedInUserId();
        //         if (teacherId == 0)
        //             return Unauthorized(new { message = "Invalid user ID" });

        //         // ✅ Verify teacher owns the exam
        //         var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
        //         if (!isOwner)
        //         {
        //             _logger.LogWarning($"Teacher {teacherId} attempted to view responses for exam {examId} they don't own");
        //             return Forbid();
        //         }

        //         // ✅ Get responses
        //         var responses = await _responseService.GetExamResponsesByQuestionAsync(questionId);

        //         _logger.LogInformation($"Teacher {teacherId} retrieved all responses for question {questionId}");
        //         return Ok(new { count = responses.Count(), data = responses });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"Error retrieving question responses: {ex.Message}");
        //         return StatusCode(StatusCodes.Status500InternalServerError,
        //             new { message = "An error occurred while retrieving responses" });
        //     }
        // }
        // !! Till here

        /// <summary>
        /// Get a specific response by ID
        /// GET /api/exams/{examId}/responses/{id}
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResponseById(int examId, int id)
        {
            try
            {
                // ✅ Validate input
                if (examId <= 0 || id <= 0)
                    return BadRequest(new { message = "Invalid exam or response ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get response
                var response = await _responseService.GetResponseByIdAsync(id);
                if (response == null)
                {
                    _logger.LogWarning($"Response {id} not found");
                    return NotFound(new { message = "Response not found" });
                }

                // ✅ Verify response belongs to this exam
                if (response.ExamID != examId)
                {
                    _logger.LogWarning($"Response {id} does not belong to exam {examId}");
                    return NotFound(new { message = "Response not found in this exam" });
                }

                // ✅ Verify student owns the response
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
    }
}