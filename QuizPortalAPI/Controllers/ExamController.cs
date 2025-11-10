using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Exam;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/exams")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ILogger<ExamController> _logger;
        private readonly AppDbContext _context;

        public ExamController(IExamService examService, ILogger<ExamController> logger, AppDbContext context)
        {
            _examService = examService;
            _logger = logger;
            _context = context;
        }

        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }

        // POST api/exams
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDTO createExamDTO)
        {
            try
            {
                if (createExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();

                // Create exam
                var createdExam = await _examService.CreateExamAsync(teacherId!.Value, createExamDTO);

                _logger.LogInformation($"Exam created successfully by teacher {teacherId}");
                return CreatedAtAction(nameof(GetExamById), new { id = createdExam.ExamID },
                    new { message = "Exam created successfully", data = createdExam });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized exam creation: {ex.Message}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid exam creation request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating exam: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the exam" });
            }
        }

        /// <summary>
        /// Get exam details by ID (for teacher who created it)
        /// GET /api/exams/{id}
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetExamById(int id)
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                if (exam.CreatedBy != teacherId)
                    return Forbid();

                _logger.LogInformation($"Teacher {teacherId} retrieved exam {id}");
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
        /// Get all exams created by the logged-in teacher
        /// GET /api/exams
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherExams()
        {
            try
            {
                var teacherId = GetLoggedInUserId()!;

                // Get teacher's exams
                var exams = await _examService.GetTeacherExamsAsync(teacherId.Value);

                _logger.LogInformation($"Teacher {teacherId} retrieved their exams list");
                return Ok(new { count = exams.Count(), data = exams });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving teacher exams: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving exams" });
            }
        }

        /// <summary>
        /// Update exam (teacher can only update their own exams)
        /// PUT /api/exams/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] UpdateExamDTO updateExamDTO)
        {
            try
            {
                if (updateExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId()!;

                var updatedExam = await _examService.UpdateExamAsync(id, teacherId.Value, updateExamDTO);
                if (updatedExam == null)
                    return NotFound(new { message = "Exam not found" });

                _logger.LogInformation($"Teacher {teacherId} updated exam {id}");
                return Ok(new { message = "Exam updated successfully", data = updatedExam });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized exam update attempt: {ex.Message}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid exam update: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning($"Null argument in exam update: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating exam {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the exam" });
            }
        }

        /// <summary>
        /// Delete exam (teacher can only delete their own exams)
        /// DELETE /api/exams/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                var teacherId = GetLoggedInUserId();
                if (teacherId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                // Delete exam (service verifies ownership)
                var result = await _examService.DeleteExamAsync(id, teacherId.Value);
                if (!result)
                    return NotFound(new { message = "Exam not found" });

                _logger.LogInformation($"Teacher {teacherId} deleted exam {id}");
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized exam deletion attempt: {ex.Message}");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Cannot delete exam: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting exam {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the exam" });
            }
        }

        /// <summary>
        /// Get exam with questions for student taking the exam
        /// GET /api/exams/{id}/student-view
        /// </summary>
        [HttpGet("{id}/student-view")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExamForStudent(int id)
        {
            try
            {
                var studentId = GetLoggedInUserId();
                if (studentId == null)
                    return Unauthorized(new { message = "Invalid or missing user ID" });

                // Get exam
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                {
                    _logger.LogWarning($"Exam {id} not found");
                    return NotFound(new { message = "Exam not found" });
                }

                var existingResult = await _context.Results
                    .FirstOrDefaultAsync(r => r.ExamID == id && r.StudentID == studentId &&
                        (r.Status == "Completed" || r.Status == "Graded"));

                if (existingResult != null)
                {
                    _logger.LogWarning($"Student {studentId} attempted to re-access already submitted exam {id}");
                    return BadRequest(new { message = "You have already submitted this exam. You cannot access it again." });
                }

                var now = DateTime.UtcNow;
                if (now < exam.ScheduleStart || now > exam.ScheduleEnd)
                {
                    _logger.LogWarning($"Student {studentId} attempted to access exam {id} outside schedule");
                    return BadRequest(new { message = "Exam is not currently available" });
                }

                // Get questions with options for this exam
                var questions = await _examService.GetExamQuestionsForStudentAsync(id);

                _logger.LogInformation($"Student {studentId} retrieved exam {id} for taking");
                return Ok(new
                {
                    data = new
                    {
                        examID = exam.ExamID,
                        title = exam.Title,
                        description = exam.Description,
                        durationMinutes = exam.DurationMinutes,
                        scheduleStart = exam.ScheduleStart,
                        scheduleEnd = exam.ScheduleEnd,
                        totalMarks = exam.TotalMarks,
                        passingMarks = exam.PassingMarks,
                        batchRemark = exam.BatchRemark,
                        questions = questions
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam for student: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the exam" });
            }
        }

        /// <summary>
        /// Student validates access to exam using access code and password
        /// POST /api/exams/validate-access
        /// </summary>
        [HttpPost("validate-access")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateAccess([FromBody] AccessExamDTO accessExamDTO)
        {
            try
            {
                if (accessExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(accessExamDTO.AccessCode))
                    return BadRequest(new { message = "Access code is required" });

                var accessResponse = await _examService.ValidateAccessAsync(accessExamDTO.AccessCode);

                if (accessResponse == null)
                    return BadRequest(new { message = "Invalid access code" });

                if (!accessResponse.CanAttempt)
                {
                    _logger.LogInformation($"Student access denied: {accessResponse.Message}");
                    return Ok(new { canAttempt = false, message = accessResponse.Message });
                }

                // Check if authenticated student has already submitted this exam
                var studentId = GetLoggedInUserId();
                if (studentId > 0 && accessResponse.ExamID > 0)
                {
                    var existingResult = await _context.Results
                        .FirstOrDefaultAsync(r => r.ExamID == accessResponse.ExamID && r.StudentID == studentId &&
                            (r.Status == "Completed" || r.Status == "Graded"));

                    if (existingResult != null)
                    {
                        _logger.LogWarning($"Student {studentId} attempted to re-access already submitted exam {accessResponse.ExamID}");
                        return Ok(new
                        {
                            canAttempt = false,
                            message = "You have already submitted this exam. You cannot access it again."
                        });
                    }
                }

                _logger.LogInformation($"Student granted access to exam {accessResponse.ExamID}");
                return Ok(new { canAttempt = true, message = accessResponse.Message, data = accessResponse });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid arguments in access validation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating exam access: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while validating access" });
            }
        }
    }
}