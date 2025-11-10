using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Question;
using QuizPortalAPI.Models;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/exams/{examId}/questions")]
    [Authorize(Roles = "Teacher")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(
            IQuestionService questionService,
            ILogger<QuestionController> logger)
        {
            _questionService = questionService;
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
        /// Teacher creates a new question for an exam
        /// POST /api/exams/{examId}/questions
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateQuestion(int examId, [FromBody] CreateQuestionDTO createQuestionDTO)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (createQuestionDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;

                var createdQuestion = await _questionService.CreateQuestionAsync(examId, teacherId.Value, createQuestionDTO);

                _logger.LogInformation($"Question created for exam {examId} by teacher {teacherId}");
                return CreatedAtAction(nameof(GetQuestionById), new { examId, id = createdQuestion.QuestionID },
                    new { message = "Question created successfully", data = createdQuestion });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating question: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the question" });
            }
        }

        /// <summary>
        /// Get all questions for an exam
        /// GET /api/exams/{examId}/questions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetExamQuestions(int examId)
        {
            try
            {
                // Validate input
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId()!;
                // Verify teacher owns the exam
                var isOwner = await _questionService.IsExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to access questions for exam {examId} they don't own");
                    return Forbid();
                }

                // Get questions
                var questions = await _questionService.GetExamQuestionsAsync(examId);

                // Get exam stats
                var questionCount = questions.Count();
                var totalMarks = await _questionService.GetExamTotalMarksAsync(examId);

                _logger.LogInformation($"Teacher {teacherId} retrieved questions for exam {examId}");
                return Ok(new
                {
                    count = questionCount,
                    totalMarks = totalMarks,
                    data = questions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam questions: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving questions" });
            }
        }

        /// <summary>
        /// Get question details by ID
        /// GET /api/exams/{examId}/questions/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(int examId, int id)
        {
            try
            {

                if (examId <= 0 || id <= 0)
                {
                    return BadRequest(new { message = "Invalid exam or question ID" }); 
                }

                var teacherId = GetLoggedInUserId()!;

                var isOwner = await _questionService.IsExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to access question from exam {examId} they don't own");
                    return Forbid();
                }

                var question = await _questionService.GetQuestionByIdAsync(id);
                if (question == null)
                {
                    _logger.LogWarning($"Question {id} not found");
                    return NotFound(new { message = "Question not found" });
                }

                if (question.ExamID != examId)
                {
                    _logger.LogWarning($"Question {id} does not belong to exam {examId}");
                    return NotFound(new { message = "Question not found in this exam" });
                }

                _logger.LogInformation($"Teacher {teacherId} retrieved question {id}");
                return Ok(new { data = question });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the question" });
            }
        }

        /// <summary>
        /// Update a question
        /// PUT /api/exams/{examId}/questions/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int examId, int id, [FromBody] UpdateQuestionDTO updateQuestionDTO)
        {
            try
            {
                // Validate input
                if (examId <= 0 || id <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                if (updateQuestionDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;

                // Verify teacher owns the exam
                var isExamOwner = await _questionService.IsExamOwnerAsync(examId, teacherId.Value);
                if (!isExamOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to update question in exam {examId} they don't own");
                    return Forbid();
                }

                // Verify teacher owns the question
                var isQuestionOwner = await _questionService.IsTeacherQuestionOwnerAsync(id, teacherId.Value);
                if (!isQuestionOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to update question {id} they don't own");
                    return Forbid();
                }

                // Update question
                var updatedQuestion = await _questionService.UpdateQuestionAsync(id, teacherId.Value, updateQuestionDTO);
                if (updatedQuestion == null)
                    return NotFound(new { message = "Question not found" });

                // Verify updated question belongs to this exam
                if (updatedQuestion.ExamID != examId)
                    return NotFound(new { message = "Question not found in this exam" });

                _logger.LogInformation($"Teacher {teacherId} updated question {id}");
                return Ok(new { message = "Question updated successfully", data = updatedQuestion });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating question {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the question" });
            }
        }

        /// <summary>
        /// Delete a question
        /// DELETE /api/exams/{examId}/questions/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int examId, int id)
        {
            try
            {
                // Validate input
                if (examId <= 0 || id <= 0)
                    return BadRequest(new { message = "Invalid exam or question ID" });

                var teacherId = GetLoggedInUserId()!;

                // Verify teacher owns the exam
                var isExamOwner = await _questionService.IsExamOwnerAsync(examId, teacherId.Value);
                if (!isExamOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to delete question from exam {examId} they don't own");
                    return Forbid();
                }

                // Verify teacher owns the question
                var isQuestionOwner = await _questionService.IsTeacherQuestionOwnerAsync(id, teacherId.Value);
                if (!isQuestionOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to delete question {id} they don't own");
                    return Forbid();
                }

                // Delete question
                var result = await _questionService.DeleteQuestionAsync(id, teacherId.Value);
                if (!result)
                    return NotFound(new { message = "Question not found" });

                _logger.LogInformation($"Teacher {teacherId} deleted question {id}");
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting question {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the question" });
            }
        }

        /// <summary>
        /// Get exam statistics (question count and total marks)
        /// GET /api/exams/{examId}/questions/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetExamStats(int examId)
        {
            try
            {
                if (examId <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId()!;

                var isOwner = await _questionService.IsExamOwnerAsync(examId, teacherId.Value);
                if (!isOwner)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to access stats for exam {examId} they don't own");
                    return Forbid();
                }

                var questionCount = await _questionService.GetExamQuestionCountAsync(examId);
                var totalMarks = await _questionService.GetExamTotalMarksAsync(examId);

                _logger.LogInformation($"Teacher {teacherId} retrieved stats for exam {examId}");
                return Ok(new
                {
                    examID = examId,
                    questionCount = questionCount,
                    totalMarks = totalMarks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam stats: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exam statistics" });
            }
        }
    }
}