using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Exam;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(AppDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Teacher creates a new exam
        /// </summary>
        public async Task<ExamResponseDTO> CreateExamAsync(int teacherId, CreateExamDTO createExamDTO)
        {
            try
            {
                // ✅ Validate teacher exists and is a teacher
                var teacher = await _context.Users.FindAsync(teacherId);
                if (teacher == null || teacher.Role != UserRole.Teacher)
                {
                    _logger.LogWarning($"Non-teacher user {teacherId} attempted to create exam");
                    throw new UnauthorizedAccessException("Only teachers can create exams");
                }

                // ✅ Validate input data
                if (createExamDTO == null)
                    throw new ArgumentNullException(nameof(createExamDTO));

                if (string.IsNullOrWhiteSpace(createExamDTO.Title))
                    throw new InvalidOperationException("Exam title is required");

                // ✅ Validate schedule: Start < End
                if (createExamDTO.ScheduleStart >= createExamDTO.ScheduleEnd)
                    throw new InvalidOperationException("Schedule start time must be before end time");

                // ✅ Validate schedule: Both times are in future
                if (createExamDTO.ScheduleStart < DateTime.UtcNow)
                    throw new InvalidOperationException("Schedule start time must be in the future");

                // ✅ Validate duration
                if (createExamDTO.DurationMinutes <= 0)
                    throw new InvalidOperationException("Duration must be greater than 0");

                // ✅ Validate duration doesn't exceed schedule window
                var scheduleWindowMinutes = (createExamDTO.ScheduleEnd - createExamDTO.ScheduleStart).TotalMinutes;
                if (createExamDTO.DurationMinutes > scheduleWindowMinutes)
                    throw new InvalidOperationException(
                        $"Exam duration ({createExamDTO.DurationMinutes} minutes) cannot exceed the time window between start and end times ({scheduleWindowMinutes:F0} minutes)");

                // ✅ Validate passing percentage
                if (createExamDTO.PassingPercentage < 0 || createExamDTO.PassingPercentage > 100)
                    throw new InvalidOperationException("Passing percentage must be between 0 and 100");

                // ✅ Validate password strength
                if (string.IsNullOrWhiteSpace(createExamDTO.AccessPassword) || createExamDTO.AccessPassword.Length < 6)
                    throw new InvalidOperationException("Access password must be at least 6 characters");

                // ✅ Generate unique access code
                string accessCode = GenerateAccessCode();
                while (await ExamExistsByAccessCodeAsync(accessCode))
                {
                    accessCode = GenerateAccessCode();
                }

                // ✅ Hash the access password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(createExamDTO.AccessPassword);

                // ✅ Create exam entity
                var exam = new Exam
                {
                    Title = createExamDTO.Title,
                    Description = createExamDTO.Description,
                    CreatedBy = teacherId,
                    BatchRemark = createExamDTO.BatchRemark,
                    DurationMinutes = createExamDTO.DurationMinutes,
                    ScheduleStart = createExamDTO.ScheduleStart,
                    ScheduleEnd = createExamDTO.ScheduleEnd,
                    PassingPercentage = createExamDTO.PassingPercentage,
                    HasNegativeMarking = createExamDTO.HasNegativeMarking,
                    AccessCode = accessCode,
                    AccessPassword = hashedPassword,
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ Save to database
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Exam '{exam.Title}' created by teacher {teacherId} with ID {exam.ExamID}");

                return MapToExamResponseDTO(exam, teacher.FullName);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Validation error in CreateExamAsync: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in CreateExamAsync: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access in CreateExamAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating exam: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get exam details by ID
        /// </summary>
        public async Task<ExamResponseDTO?> GetExamByIdAsync(int examId)
        {
            try
            {
                var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)  // Include questions for TotalMarks calculation
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return null;
                }

                _logger.LogInformation($"Exam {examId} retrieved successfully");
                return MapToExamResponseDTO(exam, exam.CreatedByUser?.FullName ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam {examId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all exams created by a teacher
        /// </summary>
        public async Task<IEnumerable<ExamListDTO>> GetTeacherExamsAsync(int teacherId)
        {
            try
            {
                var exams = await _context.Exams
                    .Include(e => e.Questions)
                    .Where(e => e.CreatedBy == teacherId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {exams.Count} exams for teacher {teacherId}");
                return exams.Select(MapToExamListDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving teacher exams: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all exams (Admin only)
        /// </summary>
        public async Task<IEnumerable<ExamListDTO>> GetAllExamsAsync()
        {
            try
            {
                var exams = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Admin retrieved all {exams.Count} exams");
                return exams.Select(MapToExamListDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all exams: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Teacher updates their exam
        /// </summary>
        public async Task<ExamResponseDTO?> UpdateExamAsync(int examId, int teacherId, UpdateExamDTO updateExamDTO)
        {
            try
            {
                if (updateExamDTO == null)
                    throw new ArgumentNullException(nameof(updateExamDTO));

                // ✅ Get exam
                var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return null;
                }

                // ✅ Verify ownership
                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to update exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only update your own exams");
                }

                // ✅ Prevent updates if exam is active or ended
                if (DateTime.UtcNow >= exam.ScheduleStart)
                {
                    _logger.LogWarning($"Attempted to update exam {examId} that is active or has ended");
                    throw new InvalidOperationException("Exam details can only be updated while the exam is upcoming. Once an exam starts, it cannot be modified.");
                }

                // ✅ Update allowed fields
                if (!string.IsNullOrWhiteSpace(updateExamDTO.Title))
                {
                    if (updateExamDTO.Title.Length < 3 || updateExamDTO.Title.Length > 200)
                        throw new InvalidOperationException("Title must be between 3 and 200 characters");
                    exam.Title = updateExamDTO.Title;
                }

                if (updateExamDTO.Description != null)
                    exam.Description = updateExamDTO.Description;

                if (updateExamDTO.BatchRemark != null)
                    exam.BatchRemark = updateExamDTO.BatchRemark;

                if (updateExamDTO.DurationMinutes.HasValue)
                {
                    if (updateExamDTO.DurationMinutes <= 0)
                        throw new InvalidOperationException("Duration must be greater than 0");
                    exam.DurationMinutes = updateExamDTO.DurationMinutes.Value;
                }

                if (updateExamDTO.ScheduleStart.HasValue && updateExamDTO.ScheduleEnd.HasValue)
                {
                    if (updateExamDTO.ScheduleStart >= updateExamDTO.ScheduleEnd)
                        throw new InvalidOperationException("Schedule start must be before end");
                    exam.ScheduleStart = updateExamDTO.ScheduleStart.Value;
                    exam.ScheduleEnd = updateExamDTO.ScheduleEnd.Value;
                }
                else if (updateExamDTO.ScheduleStart.HasValue || updateExamDTO.ScheduleEnd.HasValue)
                {
                    throw new InvalidOperationException("Both schedule start and end times must be provided together");
                }

                // ✅ Validate duration doesn't exceed schedule window (after updates)
                var scheduleWindowMinutes = (exam.ScheduleEnd - exam.ScheduleStart).TotalMinutes;
                if (exam.DurationMinutes > scheduleWindowMinutes)
                    throw new InvalidOperationException(
                        $"Exam duration ({exam.DurationMinutes} minutes) cannot exceed the time window between start and end times ({scheduleWindowMinutes:F0} minutes)");

                if (updateExamDTO.PassingPercentage.HasValue)
                {
                    if (updateExamDTO.PassingPercentage < 0 || updateExamDTO.PassingPercentage > 100)
                        throw new InvalidOperationException("Passing percentage must be between 0 and 100");
                    exam.PassingPercentage = updateExamDTO.PassingPercentage.Value;
                }

                if (updateExamDTO.HasNegativeMarking.HasValue)
                    exam.HasNegativeMarking = updateExamDTO.HasNegativeMarking.Value;

                // ✅ Update password if provided
                if (!string.IsNullOrWhiteSpace(updateExamDTO.AccessPassword))
                {
                    if (updateExamDTO.AccessPassword.Length < 6)
                        throw new InvalidOperationException("Password must be at least 6 characters");
                    exam.AccessPassword = BCrypt.Net.BCrypt.HashPassword(updateExamDTO.AccessPassword);
                }

                // ✅ Save changes
                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Exam {examId} updated by teacher {teacherId}");
                return MapToExamResponseDTO(exam, exam.CreatedByUser?.FullName ?? "Unknown");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Validation error in UpdateExamAsync: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in UpdateExamAsync: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access in UpdateExamAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating exam {examId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Teacher deletes their exam
        /// </summary>
        public async Task<bool> DeleteExamAsync(int examId, int teacherId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return false;
                }

                // ✅ Verify ownership
                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to delete exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only delete your own exams");
                }

                // ✅ Prevent deletion if exam is active or ended
                if (DateTime.UtcNow >= exam.ScheduleStart)
                {
                    _logger.LogWarning($"Attempted to delete exam {examId} that is active or has ended");
                    throw new InvalidOperationException("Exam can only be deleted while it is upcoming. Once an exam starts, it cannot be deleted.");
                }

                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Exam {examId} deleted by teacher {teacherId}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized deletion attempt: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Cannot delete active exam: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting exam {examId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Student validates access to exam
        /// </summary>
        public async Task<ExamAccessResponseDTO?> ValidateAccessAsync(string accessCode, string accessPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accessCode) || string.IsNullOrWhiteSpace(accessPassword))
                    throw new ArgumentException("Access code and password are required");

                // ✅ Find exam by access code
                var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)  // Include questions for TotalMarks calculation
                    .FirstOrDefaultAsync(e => e.AccessCode == accessCode);

                if (exam == null)
                {
                    _logger.LogWarning($"Invalid access code attempted: {accessCode}");
                    return new ExamAccessResponseDTO
                    {
                        CanAttempt = false,
                        Message = "Invalid access code"
                    };
                }

                // ✅ Verify password
                if (!BCrypt.Net.BCrypt.Verify(accessPassword, exam.AccessPassword))
                {
                    _logger.LogWarning($"Invalid password for exam {exam.ExamID}");
                    return new ExamAccessResponseDTO
                    {
                        CanAttempt = false,
                        Message = "Invalid password"
                    };
                }

                var now = DateTime.UtcNow;

                // ✅ Check if exam schedule is valid
                if (now < exam.ScheduleStart)
                {
                    _logger.LogInformation($"Exam {exam.ExamID} access attempted before start time");
                    return new ExamAccessResponseDTO
                    {
                        ExamID = exam.ExamID,
                        Title = exam.Title,
                        Description = exam.Description,
                        DurationMinutes = exam.DurationMinutes,
                        ScheduleStart = exam.ScheduleStart,
                        ScheduleEnd = exam.ScheduleEnd,
                        PassingPercentage = exam.PassingPercentage,
                        TotalMarks = exam.TotalMarks,
                        PassingMarks = exam.PassingMarks,
                        HasNegativeMarking = exam.HasNegativeMarking,
                        BatchRemark = exam.BatchRemark,
                        CreatedByUserName = exam.CreatedByUser?.FullName ?? "Unknown",
                        CanAttempt = false,
                        Message = $"Exam will start at {exam.ScheduleStart.ToLocalTime().ToString("dd MMM yyyy, hh:mm tt")}"
                    };
                }

                if (now > exam.ScheduleEnd)
                {
                    _logger.LogInformation($"Exam {exam.ExamID} access attempted after end time");
                    return new ExamAccessResponseDTO
                    {
                        ExamID = exam.ExamID,
                        Title = exam.Title,
                        CanAttempt = false,
                        Message = "Exam time has expired"
                    };
                }

                // ✅ Exam is active and accessible
                _logger.LogInformation($"Student granted access to exam {exam.ExamID}");
                return new ExamAccessResponseDTO
                {
                    ExamID = exam.ExamID,
                    Title = exam.Title,
                    Description = exam.Description,
                    DurationMinutes = exam.DurationMinutes,
                    ScheduleStart = exam.ScheduleStart,
                    ScheduleEnd = exam.ScheduleEnd,
                    PassingPercentage = exam.PassingPercentage,
                    TotalMarks = exam.TotalMarks,
                    PassingMarks = exam.PassingMarks,
                    HasNegativeMarking = exam.HasNegativeMarking,
                    BatchRemark = exam.BatchRemark,
                    CreatedByUserName = exam.CreatedByUser?.FullName ?? "Unknown",
                    CanAttempt = true,
                    Message = "Access granted. You can now start the exam."
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid arguments in ValidateAccessAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating exam access: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if teacher owns the exam
        /// </summary>
        public async Task<bool> IsTeacherExamOwnerAsync(int examId, int teacherId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                return exam != null && exam.CreatedBy == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking exam ownership: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if access code already exists
        /// </summary>
        public async Task<bool> ExamExistsByAccessCodeAsync(string accessCode)
        {
            try
            {
                return await _context.Exams.AnyAsync(e => e.AccessCode == accessCode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking access code: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate unique access code (6 alphanumeric characters)
        /// </summary>
        public string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 6)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        /// <summary>
        /// Map Exam entity to ExamResponseDTO
        /// </summary>
        private ExamResponseDTO MapToExamResponseDTO(Exam exam, string createdByUserName)
        {
            var now = DateTime.UtcNow;
            var isActive = now >= exam.ScheduleStart && now <= exam.ScheduleEnd;

            return new ExamResponseDTO
            {
                ExamID = exam.ExamID,
                Title = exam.Title,
                Description = exam.Description,
                CreatedBy = exam.CreatedBy,
                CreatedByUserName = createdByUserName,
                BatchRemark = exam.BatchRemark,
                DurationMinutes = exam.DurationMinutes,
                ScheduleStart = exam.ScheduleStart,
                ScheduleEnd = exam.ScheduleEnd,
                PassingPercentage = exam.PassingPercentage,
                TotalMarks = exam.TotalMarks,  // Computed property
                PassingMarks = exam.PassingMarks,  // Computed property
                HasNegativeMarking = exam.HasNegativeMarking,
                AccessCode = exam.AccessCode,
                CreatedAt = exam.CreatedAt,
                IsActive = isActive,
                TimeUntilStart = exam.ScheduleStart > now ? exam.ScheduleStart - now : null,
                TimeRemaining = isActive ? exam.ScheduleEnd - now : null
            };
        }

        /// <summary>
        /// Get exam questions for student taking the exam
        /// </summary>
        public async Task<dynamic?> GetExamQuestionsForStudentAsync(int examId)
        {
            try
            {
                var exam = await _context.Exams
                    .Include(e => e.Questions!)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return null;
                }

                // Map questions without revealing correct answers
                List<object> questions = new();
                
                if (exam.Questions != null)
                {
                    foreach (var q in exam.Questions)
                    {
                        List<object> options = new();
                        if (q.Options != null)
                        {
                            foreach (var o in q.Options)
                            {
                                options.Add(new
                                {
                                    optionID = o.OptionID,
                                    optionText = o.OptionText,
                                    isCorrect = false // Never reveal correct answer to student
                                });
                            }
                        }

                        questions.Add(new
                        {
                            questionID = q.QuestionID,
                            questionText = q.QuestionText,
                            questionType = (int)q.QuestionType,
                            marks = q.Marks,
                            negativeMarks = q.NegativeMarks,
                            options = options
                        });
                    }
                }

                _logger.LogInformation($"Retrieved {questions.Count} questions for exam {examId}");
                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam questions for student: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Map Exam entity to ExamListDTO
        /// </summary>
        private ExamListDTO MapToExamListDTO(Exam exam)
        {
            var now = DateTime.UtcNow;
            var isActive = now >= exam.ScheduleStart && now <= exam.ScheduleEnd;
            
            string status = "Upcoming";
            if (now > exam.ScheduleEnd)
                status = "Ended";
            else if (isActive)
                status = "Active";

            return new ExamListDTO
            {
                ExamID = exam.ExamID,
                Title = exam.Title,
                CreatedByName = exam.CreatedByUser?.FullName ?? "Unknown",
                BatchRemark = exam.BatchRemark,
                DurationMinutes = exam.DurationMinutes,
                ScheduleStart = exam.ScheduleStart,
                ScheduleEnd = exam.ScheduleEnd,
                CreatedAt = exam.CreatedAt,
                IsActive = isActive,
                Status = status,
                PassingPercentage = exam.PassingPercentage,
                TotalMarks = exam.TotalMarks,  // Computed property
                PassingMarks = exam.PassingMarks,  // Computed property
                TotalQuestions = exam.Questions?.Count ?? 0
            };
        }
    }
}