using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.DAL.ExamRepo;
using QuizPortalAPI.DAL.UserRepo;
using QuizPortalAPI.DTOs.Exam;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class ExamService : IExamService
    {
        private readonly ILogger<ExamService> _logger;
        private readonly IExamRepository _examRepository;
        private readonly IUserRepository _userRepository;
        public ExamService(ILogger<ExamService> logger, IExamRepository examRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _examRepository = examRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Teacher creates a new exam
        /// </summary>
        public async Task<ExamResponseDTO> CreateExamAsync(int teacherId, CreateExamDTO createExamDTO)
        {
            try
            {
                var teacher = await _userRepository.GetUserDetailsByIdAsync(teacherId);

                if (string.IsNullOrWhiteSpace(createExamDTO.Title))
                    throw new InvalidOperationException("Exam title is required");

                // Validate schedule: Start < End
                if (createExamDTO.ScheduleStart >= createExamDTO.ScheduleEnd)
                    throw new InvalidOperationException("Schedule start time must be before end time");

                // Validate schedule: Both times are in future
                if (createExamDTO.ScheduleStart < DateTime.UtcNow)
                    throw new InvalidOperationException("Schedule start time must be in the future");

                // Validate duration
                if (createExamDTO.DurationMinutes <= 0)
                    throw new InvalidOperationException("Duration must be greater than 0");

                // Validate duration doesn't exceed schedule window
                var scheduleWindowMinutes = (createExamDTO.ScheduleEnd - createExamDTO.ScheduleStart).TotalMinutes;
                if (createExamDTO.DurationMinutes > scheduleWindowMinutes)
                    throw new InvalidOperationException(
                        $"Exam duration ({createExamDTO.DurationMinutes} minutes) cannot exceed the time window between start and end times ({scheduleWindowMinutes:F0} minutes)");

                // Validate passing percentage
                if (createExamDTO.PassingPercentage < 0 || createExamDTO.PassingPercentage > 100)
                    throw new InvalidOperationException("Passing percentage must be between 0 and 100");

                // Generate unique access code
                string accessCode = GenerateAccessCode();

                // Create exam entity
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
                    AccessCode = accessCode,
                    CreatedAt = DateTime.UtcNow
                };

                await _examRepository.CreateAsync(exam);

                _logger.LogInformation($"Exam '{exam.Title}' created by teacher {teacherId} with ID {exam.ExamID}");

                return MapToExamResponseDTO(exam, teacher!.FullName);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in CreateExamAsync: {ex.Message}");
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
                var exam = await _examRepository.GetExamWithQuestionsByIdWithCreatorAsync(examId);
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
                var exams = await _examRepository.GetTeacherExamsWithQuestionsAsync(teacherId);

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
                var exams = await _examRepository.GetAllExamsForAdminAsync();

                _logger.LogInformation($"Admin retrieved all {exams.Count} exams");
                return exams.Select(MapToExamListDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all exams: {ex.Message}");
                throw;
            }
        }


        public async Task<ExamResponseDTO?> UpdateExamAsync(int examId, int teacherId, UpdateExamDTO updateExamDTO)
        {
            try
            {
                // Validate input
                if (updateExamDTO == null)
                    throw new ArgumentNullException(nameof(updateExamDTO));

                // Get exam with related data
                var exam = await _examRepository.GetExamByIdWithCreatorDetails(examId);

                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return null;
                }

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to update exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only update your own exams");
                }

                // Prevent updates if exam has already started
                if (DateTime.UtcNow >= exam.ScheduleStart)
                {
                    _logger.LogWarning($"Attempted to update exam {examId} that has already started or ended");
                    throw new InvalidOperationException(
                        "Exam details can only be updated while the exam is upcoming. " +
                        "Once an exam starts, it cannot be modified.");
                }

                var finalScheduleStart = updateExamDTO.ScheduleStart ?? exam.ScheduleStart;
                var finalScheduleEnd = updateExamDTO.ScheduleEnd ?? exam.ScheduleEnd;
                var finalDuration = updateExamDTO.DurationMinutes ?? exam.DurationMinutes;

                // 1. Validate schedule times are provided together (if updating)
                if (updateExamDTO.ScheduleStart.HasValue != updateExamDTO.ScheduleEnd.HasValue)
                {
                    throw new InvalidOperationException(
                        "Both schedule start and end times must be provided together. " +
                        "You cannot update only one of them.");
                }

                // 2. Ensure final schedule start is in the future
                if (finalScheduleStart <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException(
                        $"Schedule start time must be in the future. " +
                        $"Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC, " +
                        $"Requested start: {finalScheduleStart:yyyy-MM-dd HH:mm} UTC");
                }

                // 3. Add reasonable buffer - exam must be scheduled at least 15 minutes in advance (recheck)
                var minimumStartTime = DateTime.UtcNow.AddMinutes(15);
                if (finalScheduleStart < minimumStartTime)
                {
                    throw new InvalidOperationException(
                        $"Exam must be scheduled at least 15 minutes in advance.");
                }

                // 4. Ensure end is after start
                if (finalScheduleStart >= finalScheduleEnd)
                {
                    throw new InvalidOperationException(
                        $"Schedule end time must be after start time. " +
                        $"Start: {finalScheduleStart:yyyy-MM-dd HH:mm} UTC, " +
                        $"End: {finalScheduleEnd:yyyy-MM-dd HH:mm} UTC");
                }

                var scheduleWindowMinutes = (finalScheduleEnd - finalScheduleStart).TotalMinutes;

                // 7. Validate duration fits within schedule window
                if (finalDuration > scheduleWindowMinutes)
                {
                    throw new InvalidOperationException(
                        $"Exam duration ({finalDuration} minutes) cannot exceed the time window " +
                        $"between start and end times ({scheduleWindowMinutes:F0} minutes). " +
                        $"Please either reduce the duration or extend the schedule window.");
                }

                // 8. Validate title if provided
                if (!string.IsNullOrWhiteSpace(updateExamDTO.Title))
                {
                    if (updateExamDTO.Title.Length < 3)
                    {
                        throw new InvalidOperationException(
                            "Title must be at least 3 characters long");
                    }

                    if (updateExamDTO.Title.Length > 200)
                    {
                        throw new InvalidOperationException(
                            "Title cannot exceed 200 characters");
                    }
                }

                // 9. Validate passing percentage if provided
                if (updateExamDTO.PassingPercentage.HasValue)
                {
                    if (updateExamDTO.PassingPercentage < 0 || updateExamDTO.PassingPercentage > 100)
                    {
                        throw new InvalidOperationException(
                            "Passing percentage must be between 0 and 100");
                    }
                }

                // Track what changed for logging
                var changes = new List<string>();

                // Update title
                if (!string.IsNullOrWhiteSpace(updateExamDTO.Title) && exam.Title != updateExamDTO.Title)
                {
                    changes.Add($"Title: '{exam.Title}' → '{updateExamDTO.Title}'");
                    exam.Title = updateExamDTO.Title;
                }

                // Update description
                if (updateExamDTO.Description != null && exam.Description != updateExamDTO.Description)
                {
                    var oldDesc = string.IsNullOrEmpty(exam.Description) ? "[empty]" : exam.Description;
                    var newDesc = string.IsNullOrEmpty(updateExamDTO.Description) ? "[empty]" : updateExamDTO.Description;
                    changes.Add($"Description: '{oldDesc}' → '{newDesc}'");
                    exam.Description = string.IsNullOrWhiteSpace(updateExamDTO.Description)
                        ? null
                        : updateExamDTO.Description;
                }

                // Update batch remark
                if (updateExamDTO.BatchRemark != null && exam.BatchRemark != updateExamDTO.BatchRemark)
                {
                    var oldRemark = string.IsNullOrEmpty(exam.BatchRemark) ? "[empty]" : exam.BatchRemark;
                    var newRemark = string.IsNullOrEmpty(updateExamDTO.BatchRemark) ? "[empty]" : updateExamDTO.BatchRemark;
                    changes.Add($"BatchRemark: '{oldRemark}' → '{newRemark}'");
                    exam.BatchRemark = string.IsNullOrWhiteSpace(updateExamDTO.BatchRemark)
                        ? null
                        : updateExamDTO.BatchRemark;
                }

                // Update duration
                if (updateExamDTO.DurationMinutes.HasValue && exam.DurationMinutes != finalDuration)
                {
                    changes.Add($"Duration: {exam.DurationMinutes} min → {finalDuration} min");
                    exam.DurationMinutes = finalDuration;
                }

                // Update schedule
                if (updateExamDTO.ScheduleStart.HasValue && updateExamDTO.ScheduleEnd.HasValue)
                {
                    if (exam.ScheduleStart != finalScheduleStart || exam.ScheduleEnd != finalScheduleEnd)
                    {
                        changes.Add(
                            $"Schedule: [{exam.ScheduleStart:yyyy-MM-dd HH:mm} to {exam.ScheduleEnd:yyyy-MM-dd HH:mm}] → " +
                            $"[{finalScheduleStart:yyyy-MM-dd HH:mm} to {finalScheduleEnd:yyyy-MM-dd HH:mm}]");

                        exam.ScheduleStart = finalScheduleStart.ToLocalTime();
                        exam.ScheduleEnd = finalScheduleEnd.ToLocalTime();
                    }
                }

                // Update passing percentage
                if (updateExamDTO.PassingPercentage.HasValue &&
                    exam.PassingPercentage != updateExamDTO.PassingPercentage.Value)
                {
                    changes.Add($"PassingPercentage: {exam.PassingPercentage}% → {updateExamDTO.PassingPercentage}%");
                    exam.PassingPercentage = updateExamDTO.PassingPercentage.Value;
                }

                // Check if any changes were made
                if (changes.Count == 0)
                {
                    _logger.LogInformation($"No changes detected for exam {examId} by teacher {teacherId}");
                    return MapToExamResponseDTO(exam, exam.CreatedByUser?.FullName ?? "Unknown");
                }

                await _examRepository.UpdateAsync(exam);

                // Log successful update with details
                _logger.LogInformation(
                    $"Exam {examId} updated by teacher {teacherId}. Changes: {string.Join("; ", changes)}");

                return MapToExamResponseDTO(exam, exam.CreatedByUser?.FullName ?? "Unknown");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Validation error in UpdateExamAsync for exam {examId}: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in UpdateExamAsync for exam {examId}: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access in UpdateExamAsync for exam {examId}: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error updating exam {examId}");
                throw new InvalidOperationException("Failed to save exam changes to database. Please try again.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating exam {examId}");
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
                var exam = await _examRepository.GetExamDetailsByIdAsync(examId);
                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return false;
                }

                // Verify ownership
                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to delete exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only delete your own exams");
                }

                // Prevent deletion if exam is active or ended
                if (DateTime.UtcNow >= exam.ScheduleStart)
                {
                    _logger.LogWarning($"Attempted to delete exam {examId} that is active or has ended");
                    throw new InvalidOperationException("Exam can only be deleted while it is upcoming. Once an exam starts, it cannot be deleted.");
                }

                await _examRepository.DeleteAsync(exam);

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
        public async Task<ExamAccessResponseDTO?> ValidateAccessAsync(string accessCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accessCode))
                    throw new ArgumentException("Access code is required");

                var exam = await _examRepository.GetExamByAccessCodeAsync(accessCode);

                if (exam == null)
                {
                    _logger.LogWarning($"Invalid access code attempted: {accessCode}");
                    return new ExamAccessResponseDTO
                    {
                        CanAttempt = false,
                        Message = "Invalid access code"
                    };
                }

                var now = DateTime.UtcNow;

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

                var timeRemaining = exam.ScheduleEnd - now;

                if (timeRemaining < TimeSpan.FromMinutes(exam.DurationMinutes))
                {
                    _logger.LogInformation($"Insufficient time remaining to start exam {exam.ExamID}");
                    return new ExamAccessResponseDTO
                    {
                        ExamID = exam.ExamID,
                        Title = exam.Title,
                        CanAttempt = false,
                        Message = "Insufficient time remaining to start the exam"
                    };
                }


                _logger.LogInformation($"Student granted access to exam {exam.ExamID}");
                return new ExamAccessResponseDTO
                {
                    ExamID = exam.ExamID,
                    Title = exam.Title,
                    Description = exam.Description,
                    DurationMinutes = timeRemaining.TotalMinutes < exam.DurationMinutes
                        ? (int)timeRemaining.TotalMinutes
                        : exam.DurationMinutes,
                    ScheduleStart = exam.ScheduleStart,
                    ScheduleEnd = exam.ScheduleEnd,
                    PassingPercentage = exam.PassingPercentage,
                    TotalMarks = exam.TotalMarks,
                    PassingMarks = exam.PassingMarks,
                    BatchRemark = exam.BatchRemark,
                    CreatedByUserName = exam.CreatedByUser?.FullName ?? "Unknown",
                    CanAttempt = true,
                    Message = timeRemaining.TotalMinutes < exam.DurationMinutes ? "You can start the exam now but will have limited time to complete it." : "Access granted. You can now start the exam."
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
                var exam = await _examRepository.GetExamDetailsByIdAsync(examId);
                return exam != null && exam.CreatedBy == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking exam ownership: {ex.Message}");
                return false;
            }
        }

        public string GenerateAccessCode()
        {
            Guid accessCode = Guid.NewGuid();
            return accessCode.ToString();
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
                AccessCode = exam.AccessCode,
                CreatedAt = exam.CreatedAt,
                // UpdatedAt = exam.UpdatedAt,
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
                var exam = await _examRepository.GetExamWithQuestionsForStudentAsync(examId);

                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found");
                    return null;
                }

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
                                    isCorrect = false // not to show the actual answer
                                });
                            }
                        }

                        questions.Add(new
                        {
                            questionID = q.QuestionID,
                            questionText = q.QuestionText,
                            questionType = (int)q.QuestionType,
                            marks = q.Marks,
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