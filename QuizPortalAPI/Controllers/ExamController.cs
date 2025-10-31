using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ExamController(IExamService examService, ILogger<ExamController> logger)
        {
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
        /// Teacher creates a new exam
        /// POST /api/exams
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDTO createExamDTO)
        {
            try
            {
                // ✅ Validate input
                if (createExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Create exam
                var createdExam = await _examService.CreateExamAsync(teacherId, createExamDTO);

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
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Argument error in exam creation: {ex.Message}");
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExamById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get exam
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                    return NotFound(new { message = "Exam not found" });

                // ✅ Verify ownership (teacher can only see their own exams)
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeacherExams()
        {
            try
            {
                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get teacher's exams
                var exams = await _examService.GetTeacherExamsAsync(teacherId);

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
        /// Get all exams (Admin only)
        /// GET /api/exams/all
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllExams()
        {
            try
            {
                var userId = GetLoggedInUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get all exams
                var exams = await _examService.GetAllExamsAsync();

                _logger.LogInformation($"Admin {userId} retrieved all exams");
                return Ok(new { count = exams.Count(), data = exams });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all exams: {ex.Message}");
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] UpdateExamDTO updateExamDTO)
        {
            try
            {
                // ✅ Validate input
                if (id <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                if (updateExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Update exam (service verifies ownership)
                var updatedExam = await _examService.UpdateExamAsync(id, teacherId, updateExamDTO);
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                // ✅ Validate input
                if (id <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var teacherId = GetLoggedInUserId();
                if (teacherId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Delete exam (service verifies ownership)
                var result = await _examService.DeleteExamAsync(id, teacherId);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExamForStudent(int id)
        {
            try
            {
                // ✅ Validate input
                if (id <= 0)
                    return BadRequest(new { message = "Invalid exam ID" });

                var studentId = GetLoggedInUserId();
                if (studentId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                // ✅ Get exam
                var exam = await _examService.GetExamByIdAsync(id);
                if (exam == null)
                {
                    _logger.LogWarning($"Exam {id} not found");
                    return NotFound(new { message = "Exam not found" });
                }

                // ✅ Check if exam is accessible (within schedule)
                var now = DateTime.UtcNow;
                if (now < exam.ScheduleStart || now > exam.ScheduleEnd)
                {
                    _logger.LogWarning($"Student {studentId} attempted to access exam {id} outside schedule");
                    return BadRequest(new { message = "Exam is not currently available" });
                }

                // ✅ Get questions with options for this exam
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
                        hasNegativeMarking = exam.HasNegativeMarking,
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateAccess([FromBody] AccessExamDTO accessExamDTO)
        {
            try
            {
                // ✅ Validate input
                if (accessExamDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(accessExamDTO.AccessCode))
                    return BadRequest(new { message = "Access code is required" });

                if (string.IsNullOrWhiteSpace(accessExamDTO.AccessPassword))
                    return BadRequest(new { message = "Access password is required" });

                // ✅ Validate access
                var accessResponse = await _examService.ValidateAccessAsync(
                    accessExamDTO.AccessCode, 
                    accessExamDTO.AccessPassword);

                if (accessResponse == null)
                    return BadRequest(new { message = "Invalid access code or password" });

                if (!accessResponse.CanAttempt)
                {
                    _logger.LogInformation($"Student access denied: {accessResponse.Message}");
                    return Ok(new { canAttempt = false, message = accessResponse.Message });
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