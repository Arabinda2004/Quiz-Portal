using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Grading;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class GradingService : IGradingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GradingService> _logger;
        private readonly IExamService _examService;

        public GradingService(AppDbContext context, ILogger<GradingService> logger, IExamService examService)
        {
            _context = context;
            _logger = logger;
            _examService = examService;
        }

        /// <summary>
        /// Get all pending responses for an exam
        /// </summary>
        public async Task<PendingResponsesDTO> GetPendingResponsesAsync(int examId, int teacherId, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                // ✅ Get all student responses (both graded and pending)
                var allResponses = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Student)
                    .Where(sr => sr.ExamID == examId)
                    .OrderByDescending(r => r.SubmittedAt)
                    .ToListAsync();

                // ✅ Get grading records to determine status
                var gradingRecords = await _context.GradingRecords
                    .Where(gr => allResponses.Select(sr => sr.ResponseID).Contains(gr.ResponseID) && gr.Status == "Graded")
                    .ToDictionaryAsync(gr => gr.ResponseID, gr => gr);

                // ✅ Filter for pending responses only
                var pendingResponses = allResponses.Where(sr => !gradingRecords.ContainsKey(sr.ResponseID)).ToList();

                var totalPending = pendingResponses.Count;
                var paginatedResponses = allResponses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var uniqueStudents = allResponses.Select(sr => sr.StudentID).Distinct().Count();

                var result = new PendingResponsesDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalPending = totalPending,
                    TotalResponses = uniqueStudents,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)allResponses.Count / pageSize),
                    Responses = paginatedResponses.Select(sr =>
                    {
                        var isGraded = gradingRecords.ContainsKey(sr.ResponseID);
                        var marksObtained = isGraded ? gradingRecords[sr.ResponseID].MarksObtained : 0;
                        
                        return new PendingResponseItemDTO
                        {
                            ResponseId = sr.ResponseID,
                            QuestionId = sr.QuestionID,
                            QuestionText = sr.Question?.QuestionText ?? "Unknown",
                            StudentId = sr.StudentID,
                            StudentName = sr.Student?.FullName ?? "Unknown",
                            StudentEmail = sr.Student?.Email ?? "Unknown",
                            StudentAnswer = sr.AnswerText,
                            MaxMarks = sr.Question?.Marks ?? 0,
                            MarksObtained = marksObtained,
                            IsGraded = isGraded,
                            SubmittedAt = sr.SubmittedAt,
                            QuestionType = sr.Question?.QuestionType.ToString() ?? "Unknown",
                            PendingQuestionsFromStudent = pendingResponses.Count(r => r.StudentID == sr.StudentID)
                        };
                    }).ToList()
                };

                _logger.LogInformation($"Retrieved {totalPending} pending responses for exam {examId}");
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get pending responses for a specific question
        /// </summary>
        public async Task<PendingResponsesDTO> GetPendingResponsesByQuestionAsync(int examId, int questionId, int teacherId, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null || question.ExamID != examId)
                    throw new InvalidOperationException("Question not found in this exam");

                // ✅ Get pending responses for this question
                var query = _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.QuestionID == questionId)
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Student)
                    .AsQueryable();

                var allResponses = await query.ToListAsync();
                var pendingResponses = allResponses
                    .Where(sr => _context.GradingRecords
                        .Where(gr => gr.ResponseID == sr.ResponseID && gr.Status == "Graded")
                        .FirstOrDefault() == null)
                    .ToList();

                var totalPending = pendingResponses.Count;
                var paginatedResponses = pendingResponses
                    .OrderByDescending(r => r.SubmittedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PendingResponsesDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalPending = totalPending,
                    TotalResponses = allResponses.Count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalPending / pageSize),
                    Responses = paginatedResponses.Select(sr => new PendingResponseItemDTO
                    {
                        ResponseId = sr.ResponseID,
                        QuestionId = sr.QuestionID,
                        QuestionText = sr.Question?.QuestionText ?? "Unknown",
                        StudentId = sr.StudentID,
                        StudentName = sr.Student?.FullName ?? "Unknown",
                        StudentEmail = sr.Student?.Email ?? "Unknown",
                        StudentAnswer = sr.AnswerText,
                        MaxMarks = sr.Question?.Marks ?? 0,
                        SubmittedAt = sr.SubmittedAt,
                        QuestionType = sr.Question?.QuestionType.ToString() ?? "Unknown",
                        PendingQuestionsFromStudent = pendingResponses.Count(r => r.StudentID == sr.StudentID)
                    }).ToList()
                };

                _logger.LogInformation($"Retrieved {totalPending} pending responses for question {questionId}");
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending responses by question: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get pending responses for a specific student
        /// </summary>
        public async Task<PendingResponsesDTO> GetPendingResponsesByStudentAsync(int examId, int studentId, int teacherId, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var student = await _context.Users.FindAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");

                // ✅ Get pending responses for this student (subjective questions without grading)
                var pendingResponses = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Student)
                    .Where(sr => sr.ExamID == examId &&
                                 sr.StudentID == studentId &&
                                 (sr.Question!.QuestionType == QuestionType.SAQ ||
                                  sr.Question!.QuestionType == QuestionType.Subjective) &&
                                 !_context.GradingRecords
                                     .Any(gr => gr.ResponseID == sr.ResponseID && gr.Status == "Graded"))
                    .OrderByDescending(r => r.SubmittedAt)
                    .ToListAsync();

                // ✅ Get all responses for total count
                var allResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .ToListAsync();

                var totalPending = pendingResponses.Count;
                var paginatedResponses = pendingResponses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PendingResponsesDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalPending = totalPending,
                    TotalResponses = allResponses.Count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalPending / pageSize),
                    Responses = paginatedResponses.Select(sr => new PendingResponseItemDTO
                    {
                        ResponseId = sr.ResponseID,
                        QuestionId = sr.QuestionID,
                        QuestionText = sr.Question?.QuestionText ?? "Unknown",
                        StudentId = sr.StudentID,
                        StudentName = sr.Student?.FullName ?? "Unknown",
                        StudentEmail = sr.Student?.Email ?? "Unknown",
                        StudentAnswer = sr.AnswerText,
                        MaxMarks = sr.Question?.Marks ?? 0,
                        SubmittedAt = sr.SubmittedAt,
                        QuestionType = sr.Question?.QuestionType.ToString() ?? "Unknown",
                        PendingQuestionsFromStudent = pendingResponses.Count(r => r.StudentID == sr.StudentID)
                    }).ToList()
                };

                _logger.LogInformation($"Retrieved {totalPending} pending responses for student {studentId}");
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting pending responses by student: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a specific response for grading
        /// </summary>
        public async Task<ResponseForGradingDTO?> GetResponseForGradingAsync(int responseId, int teacherId)
        {
            try
            {
                var response = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Exam)
                    .Include(sr => sr.Student)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);

                if (response == null)
                {
                    _logger.LogWarning($"Response {responseId} not found");
                    return null;
                }

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                // ✅ Get existing grading record if any
                var gradingRecord = await _context.GradingRecords
                    .Include(gr => gr.GradedByTeacher)
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");

                return new ResponseForGradingDTO
                {
                    ResponseId = response.ResponseID,
                    ExamId = response.ExamID,
                    QuestionId = response.QuestionID,
                    StudentId = response.StudentID,
                    StudentName = response.Student?.FullName ?? "Unknown",
                    StudentEmail = response.Student?.Email ?? "Unknown",
                    QuestionText = response.Question?.QuestionText ?? "Unknown",
                    QuestionType = response.Question?.QuestionType.ToString() ?? "Unknown",
                    MaxMarks = response.Question?.Marks ?? 0,
                    StudentAnswer = response.AnswerText,
                    CurrentMarksObtained = gradingRecord?.MarksObtained,
                    SubmittedAt = response.SubmittedAt,
                    IsGraded = gradingRecord != null,
                    GradedAt = gradingRecord?.GradedAt,
                    GradedByTeacher = gradingRecord?.GradedByTeacher?.FullName,
                    Feedback = gradingRecord?.Feedback
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting response for grading: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Grade a single response
        /// </summary>
        public async Task<bool> GradeSingleResponseAsync(int responseId, int teacherId, GradeSingleResponseDTO gradeDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ Get the response
                var response = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Exam)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);

                if (response == null)
                    throw new InvalidOperationException("Response not found");

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < response.Exam!.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {response.Exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                // ✅ Check if exam is published - prevent grading after publication
                var isPublished = await IsExamPublishedAsync(response.ExamID);
                if (isPublished)
                    throw new InvalidOperationException("Cannot update marks for a published exam. Please unpublish the exam first to make changes.");

                // ✅ Validate marks against max marks
                if (gradeDto.MarksObtained > response.Question?.Marks)
                    throw new ArgumentException($"Marks cannot exceed {response.Question?.Marks}");

                // ✅ Remove old grading record if exists
                var oldGrading = await _context.GradingRecords
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");
                if (oldGrading != null)
                {
                    _context.GradingRecords.Remove(oldGrading);
                    await _context.SaveChangesAsync();
                }

                // ✅ Update response with marks
                response.MarksObtained = gradeDto.MarksObtained;
                // IsCorrect indicates if the student got marks (any marks > 0 = partially/fully correct)
                // A grade of 0 means incorrect answer. The response is still considered "graded"
                // because a GradingRecord with Status="Graded" will be created
                response.IsCorrect = gradeDto.MarksObtained > 0;

                _context.StudentResponses.Update(response);
                await _context.SaveChangesAsync();

                // ✅ Create grading record
                var gradingRecord = new GradingRecord
                {
                    ResponseID = responseId,
                    QuestionID = response.QuestionID,
                    StudentID = response.StudentID,
                    GradedByTeacherID = teacherId,
                    MarksObtained = gradeDto.MarksObtained,
                    Feedback = gradeDto.Feedback,
                    Comment = gradeDto.Comment,
                    IsPartialCredit = gradeDto.IsPartialCredit,
                    Status = "Graded",
                    GradedAt = DateTime.UtcNow
                };

                _context.GradingRecords.Add(gradingRecord);
                await _context.SaveChangesAsync();

                // ✅ Recalculate result for the student
                await RecalculateStudentResultAsync(response.ExamID, response.StudentID);

                await transaction.CommitAsync();
                _logger.LogInformation($"Response {responseId} graded by teacher {teacherId} with {gradeDto.MarksObtained} marks");
                return true;
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Validation error: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error grading response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Grade multiple responses in a batch
        /// </summary>
        public async Task<bool> GradeBatchResponsesAsync(int teacherId, BatchGradeDTO batchGradeDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(batchGradeDto.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                var exam = await _context.Exams.FindAsync(batchGradeDto.ExamID);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                // ✅ Check if exam is published - prevent batch grading after publication
                var isPublished = await IsExamPublishedAsync(batchGradeDto.ExamID);
                if (isPublished)
                    throw new InvalidOperationException("Cannot grade responses for a published exam. Please unpublish the exam first to make changes.");

                var question = await _context.Questions.FindAsync(batchGradeDto.QuestionID);
                if (question == null || question.ExamID != batchGradeDto.ExamID)
                    throw new InvalidOperationException("Question not found in this exam");

                // ✅ Process each response
                var affectedStudents = new HashSet<int>();
                foreach (var gradeItem in batchGradeDto.Responses)
                {
                    // Get response
                    var response = await _context.StudentResponses
                        .Include(sr => sr.Question)
                        .FirstOrDefaultAsync(sr => sr.ResponseID == gradeItem.ResponseID);

                    if (response == null)
                    {
                        _logger.LogWarning($"Response {gradeItem.ResponseID} not found, skipping");
                        continue;
                    }

                    // ✅ Validate marks
                    if (gradeItem.MarksObtained > response.Question?.Marks)
                        throw new ArgumentException($"Marks for response {gradeItem.ResponseID} cannot exceed {response.Question?.Marks}");

                    // Remove old grading if exists
                    var oldGrading = await _context.GradingRecords
                        .FirstOrDefaultAsync(gr => gr.ResponseID == gradeItem.ResponseID && gr.Status == "Graded");
                    if (oldGrading != null)
                        _context.GradingRecords.Remove(oldGrading);

                    // Update response
                    response.MarksObtained = gradeItem.MarksObtained;
                    // IsCorrect indicates if the student got marks (any marks > 0 = partially/fully correct)
                    // A grade of 0 means incorrect answer. The response is still "graded"
                    response.IsCorrect = gradeItem.MarksObtained > 0;
                    _context.StudentResponses.Update(response);

                    // Create grading record
                    var gradingRecord = new GradingRecord
                    {
                        ResponseID = gradeItem.ResponseID,
                        QuestionID = response.QuestionID,
                        StudentID = response.StudentID,
                        GradedByTeacherID = teacherId,
                        MarksObtained = gradeItem.MarksObtained,
                        Feedback = gradeItem.Feedback,
                        Comment = gradeItem.Comment,
                        Status = "Graded",
                        GradedAt = DateTime.UtcNow
                    };
                    _context.GradingRecords.Add(gradingRecord);
                    affectedStudents.Add(response.StudentID);
                }

                await _context.SaveChangesAsync();

                // ✅ Recalculate results for affected students
                foreach (var studentId in affectedStudents)
                {
                    await RecalculateStudentResultAsync(batchGradeDto.ExamID, studentId);
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Batch graded {batchGradeDto.Responses.Count} responses by teacher {teacherId}");
                return true;
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Validation error: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error grading batch responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get grading history for a response
        /// </summary>
        public async Task<IEnumerable<GradingRecordDTO>> GetGradingHistoryAsync(int responseId)
        {
            try
            {
                var records = await _context.GradingRecords
                    .Where(gr => gr.ResponseID == responseId)
                    .Include(gr => gr.GradedByTeacher)
                    .Include(gr => gr.Student)
                    .OrderByDescending(gr => gr.GradedAt)
                    .ToListAsync();

                return records.Select(gr => new GradingRecordDTO
                {
                    GradingId = gr.GradingID,
                    ResponseId = gr.ResponseID,
                    QuestionId = gr.QuestionID,
                    StudentId = gr.StudentID,
                    StudentName = gr.Student?.FullName ?? "Unknown",
                    GradedByTeacherId = gr.GradedByTeacherID,
                    GradedByTeacher = gr.GradedByTeacher?.FullName ?? "Unknown",
                    MarksObtained = gr.MarksObtained,
                    Feedback = gr.Feedback,
                    Comment = gr.Comment,
                    GradedAt = gr.GradedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting grading history: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get grading history for all responses of a question
        /// </summary>
        public async Task<IEnumerable<GradingRecordDTO>> GetQuestionGradingHistoryAsync(int examId, int questionId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view grading history for your own exams");

                var records = await _context.GradingRecords
                    .Where(gr => gr.QuestionID == questionId)
                    .Include(gr => gr.GradedByTeacher)
                    .Include(gr => gr.Student)
                    .OrderByDescending(gr => gr.GradedAt)
                    .ToListAsync();

                return records.Select(gr => new GradingRecordDTO
                {
                    GradingId = gr.GradingID,
                    ResponseId = gr.ResponseID,
                    QuestionId = gr.QuestionID,
                    StudentId = gr.StudentID,
                    StudentName = gr.Student?.FullName ?? "Unknown",
                    GradedByTeacherId = gr.GradedByTeacherID,
                    GradedByTeacher = gr.GradedByTeacher?.FullName ?? "Unknown",
                    MarksObtained = gr.MarksObtained,
                    Feedback = gr.Feedback,
                    Comment = gr.Comment,
                    GradedAt = gr.GradedAt
                }).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting question grading history: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get grading history for a student
        /// </summary>
        public async Task<IEnumerable<GradingRecordDTO>> GetStudentGradingHistoryAsync(int examId, int studentId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view grading history for your own exams");

                var records = await _context.GradingRecords
                    .Where(gr => gr.StudentID == studentId)
                    .Include(gr => gr.GradedByTeacher)
                    .Include(gr => gr.Student)
                    .OrderByDescending(gr => gr.GradedAt)
                    .ToListAsync();

                return records.Select(gr => new GradingRecordDTO
                {
                    GradingId = gr.GradingID,
                    ResponseId = gr.ResponseID,
                    QuestionId = gr.QuestionID,
                    StudentId = gr.StudentID,
                    StudentName = gr.Student?.FullName ?? "Unknown",
                    GradedByTeacherId = gr.GradedByTeacherID,
                    GradedByTeacher = gr.GradedByTeacher?.FullName ?? "Unknown",
                    MarksObtained = gr.MarksObtained,
                    Feedback = gr.Feedback,
                    Comment = gr.Comment,
                    GradedAt = gr.GradedAt
                }).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student grading history: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get grading statistics for an exam
        /// </summary>
        public async Task<GradingStatsDTO> GetGradingStatsAsync(int examId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view statistics for your own exams");

                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // Get all questions in the exam
                var questions = await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .ToListAsync();

                var questionsWithResponses = new List<QuestionGradingStatsDTO>();
                int totalResponses = 0;
                int totalGraded = 0;

                foreach (var question in questions)
                {
                    var responses = await _context.StudentResponses
                        .Where(sr => sr.QuestionID == question.QuestionID)
                        .ToListAsync();

                    var gradedCount = await _context.GradingRecords
                        .CountAsync(gr => gr.QuestionID == question.QuestionID && gr.Status == "Graded");

                    var averageMarks = gradedCount > 0
                        ? await _context.GradingRecords
                            .Where(gr => gr.QuestionID == question.QuestionID && gr.Status == "Graded")
                            .AverageAsync(gr => (double)gr.MarksObtained)
                        : 0;

                    questionsWithResponses.Add(new QuestionGradingStatsDTO
                    {
                        QuestionID = question.QuestionID,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType.ToString(),
                        TotalResponses = responses.Count,
                        GradedResponses = gradedCount,
                        PendingResponses = responses.Count - gradedCount,
                        AverageMarks = (decimal)averageMarks,
                        MaxMarks = question.Marks
                    });

                    totalResponses += responses.Count;
                    totalGraded += gradedCount;
                }

                // ✅ TotalResponses = Count of unique students who responded (attempted exam)
                var uniqueStudentsResponded = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .Select(sr => sr.StudentID)
                    .Distinct()
                    .CountAsync();

                // ✅ GradedResponses = Count of students who have ALL their questions graded
                // A student is fully graded if they have grading records for all their responses
                var studentsFullyGraded = 0;
                if (uniqueStudentsResponded > 0)
                {
                    var allStudentResponses = await _context.StudentResponses
                        .Where(sr => sr.ExamID == examId)
                        .ToListAsync();

                    var studentGradingCounts = allStudentResponses
                        .GroupBy(sr => sr.StudentID)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var studentId in studentGradingCounts.Keys)
                    {
                        var totalQuestionsForStudent = studentGradingCounts[studentId];
                        var gradedQuestionsForStudent = await _context.GradingRecords
                            .Where(gr => gr.StudentID == studentId && gr.QuestionID != 0)
                            .Join(
                                _context.StudentResponses.Where(sr => sr.ExamID == examId),
                                gr => gr.ResponseID,
                                sr => sr.ResponseID,
                                (gr, sr) => gr
                            )
                            .Where(gr => gr.Status == "Graded")
                            .CountAsync();

                        if (gradedQuestionsForStudent == totalQuestionsForStudent)
                        {
                            studentsFullyGraded++;
                        }
                    }
                }

                return new GradingStatsDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalQuestions = questions.Count,
                    TotalStudents = await _context.Results
                        .Where(r => r.ExamID == examId)
                        .Select(r => r.StudentID)
                        .Distinct()
                        .CountAsync(),
                    TotalResponses = uniqueStudentsResponded,
                    GradedResponses = studentsFullyGraded,
                    PendingResponses = uniqueStudentsResponded - studentsFullyGraded,
                    GradingPercentage = uniqueStudentsResponded > 0 ? ((decimal)studentsFullyGraded / uniqueStudentsResponded) * 100 : 0,
                    QuestionStats = questionsWithResponses
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting grading statistics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mark a question as fully graded
        /// </summary>
        public async Task<bool> MarkQuestionGradedAsync(int examId, int questionId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null || question.ExamID != examId)
                    throw new InvalidOperationException("Question not found in this exam");

                // Check if all responses for this question are graded
                var totalResponses = await _context.StudentResponses
                    .CountAsync(sr => sr.QuestionID == questionId);

                var gradedResponses = await _context.GradingRecords
                    .CountAsync(gr => gr.QuestionID == questionId && gr.Status == "Graded");

                if (totalResponses != gradedResponses)
                    throw new InvalidOperationException("Cannot mark question as graded. Some responses are still pending.");

                _logger.LogInformation($"Question {questionId} marked as graded by teacher {teacherId}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking question as graded: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all graded responses for a student
        /// </summary>
        public async Task<StudentGradedResponsesDTO> GetStudentGradedResponsesAsync(int examId, int studentId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var student = await _context.Users.FindAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");

                // Get all responses for this student
                var responses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Include(sr => sr.Question)
                    .ToListAsync();

                var gradedResponses = new List<GradedResponseItemDTO>();
                decimal totalMarksObtained = 0;
                decimal totalMaxMarks = 0;

                foreach (var response in responses)
                {
                    var gradingRecord = await _context.GradingRecords
                        .Include(gr => gr.GradedByTeacher)
                        .FirstOrDefaultAsync(gr => gr.ResponseID == response.ResponseID && gr.Status == "Graded");

                    if (gradingRecord != null)
                    {
                        var marksObtained = gradingRecord.MarksObtained;
                        var maxMarks = response.Question?.Marks ?? 0;

                        totalMarksObtained += marksObtained;
                        totalMaxMarks += maxMarks;

                        gradedResponses.Add(new GradedResponseItemDTO
                        {
                            ResponseID = response.ResponseID,
                            QuestionID = response.QuestionID,
                            QuestionText = response.Question?.QuestionText ?? "Unknown",
                            QuestionType = response.Question?.QuestionType.ToString() ?? "Unknown",
                            MaxMarks = maxMarks,
                            MarksObtained = marksObtained,
                            StudentAnswer = response.AnswerText,
                            Feedback = gradingRecord.Feedback,
                            Comment = gradingRecord.Comment,
                            GradedAt = gradingRecord.GradedAt,
                            GradedBy = gradingRecord.GradedByTeacher?.FullName ?? "Unknown"
                        });
                    }
                }

                return new StudentGradedResponsesDTO
                {
                    StudentID = studentId,
                    StudentName = student.FullName,
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalMarksObtained = totalMarksObtained,
                    TotalMaxMarks = totalMaxMarks,
                    Percentage = totalMaxMarks > 0 ? (totalMarksObtained / totalMaxMarks) * 100 : 0,
                    TotalQuestions = responses.Count,
                    GradedResponses = gradedResponses
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student graded responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Regrade a response with reason tracking
        /// </summary>
        public async Task<bool> RegradeResponseAsync(int responseId, int teacherId, RegradingDTO regradingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ Get the response
                var response = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Exam)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);

                if (response == null)
                    throw new InvalidOperationException("Response not found");

                // ✅ Verify teacher owns the exam
                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only regrade responses for your own exams");

                // ✅ Check if exam has ended - allow grading only after exam end time
                if (DateTime.UtcNow < response.Exam!.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {response.Exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                // ✅ Check if exam is published - prevent regrading after publication
                var isPublished = await IsExamPublishedAsync(response.ExamID);
                if (isPublished)
                    throw new InvalidOperationException("Cannot regrade a published exam. Please unpublish the exam first to make changes.");

                // ✅ Get old grading record
                var oldGrading = await _context.GradingRecords
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");

                if (oldGrading == null)
                    throw new InvalidOperationException("No grading record found to regrade");

                // ✅ Mark old grading as regraded
                oldGrading.Status = "Regraded";
                _context.GradingRecords.Update(oldGrading);
                await _context.SaveChangesAsync();

                // ✅ Validate new marks
                if (regradingDto.NewMarksObtained > response.Question?.Marks)
                    throw new ArgumentException($"Marks cannot exceed {response.Question?.Marks}");

                // ✅ Update response with new marks
                response.MarksObtained = regradingDto.NewMarksObtained;
                // IsCorrect indicates if the student got marks (any marks > 0 = partially/fully correct)
                // A grade of 0 means incorrect answer. The response is still "graded"
                response.IsCorrect = regradingDto.NewMarksObtained > 0;
                _context.StudentResponses.Update(response);
                await _context.SaveChangesAsync();

                // ✅ Create new grading record for regrade
                var newGrading = new GradingRecord
                {
                    ResponseID = responseId,
                    QuestionID = response.QuestionID,
                    StudentID = response.StudentID,
                    GradedByTeacherID = teacherId,
                    MarksObtained = regradingDto.NewMarksObtained,
                    Feedback = regradingDto.NewFeedback,
                    Comment = regradingDto.Comment,
                    Status = "Graded",
                    RegradeFrom = oldGrading.GradingID,
                    RegradeReason = regradingDto.Reason,
                    GradedAt = DateTime.UtcNow,
                    RegradeAt = DateTime.UtcNow
                };

                _context.GradingRecords.Add(newGrading);
                await _context.SaveChangesAsync();

                // ✅ Recalculate student result
                await RecalculateStudentResultAsync(response.ExamID, response.StudentID);

                await transaction.CommitAsync();
                _logger.LogInformation($"Response {responseId} regraded by teacher {teacherId}. Old marks: {oldGrading.MarksObtained}, New marks: {regradingDto.NewMarksObtained}");
                return true;
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Validation error: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error regrading response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Helper method to recalculate student result
        /// </summary>
        private async Task<bool> RecalculateStudentResultAsync(int examId, int studentId)
        {
            try
            {
                // Get or create result
                var result = await _context.Results
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

                if (result == null)
                {
                    result = new Result
                    {
                        ExamID = examId,
                        StudentID = studentId,
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Results.Add(result);
                }

                // Get all student responses for this exam
                var studentResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Select(sr => sr.ResponseID)
                    .ToListAsync();

                // Check how many responses are graded
                var gradedResponseIds = await _context.GradingRecords
                    .Where(gr => studentResponses.Contains(gr.ResponseID) && gr.Status == "Graded")
                    .Select(gr => gr.ResponseID)
                    .Distinct()
                    .ToListAsync();

                // Check if all responses are graded
                var allResponsesGraded = studentResponses.Count > 0 && studentResponses.Count == gradedResponseIds.Count;

                // Calculate total marks
                var totalMarks = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .SumAsync(sr => sr.MarksObtained);

                // Get exam total marks
                var examTotalMarks = await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .SumAsync(q => q.Marks);

                result.TotalMarks = totalMarks;
                result.Percentage = examTotalMarks > 0 ? (totalMarks / examTotalMarks) * 100 : 0;
                result.UpdatedAt = DateTime.UtcNow;
                
                // Update status based on grading completion
                result.Status = allResponsesGraded ? "Graded" : "Completed";

                // Calculate rank
                var rank = await _context.Results
                    .Where(r => r.ExamID == examId && r.TotalMarks > totalMarks)
                    .CountAsync();
                result.Rank = rank + 1;

                _context.Results.Update(result);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recalculating student result: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper method to check if exam is published
        /// </summary>
        private async Task<bool> IsExamPublishedAsync(int examId)
        {
            try
            {
                var publication = await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId && ep.Status == "Published");

                return publication != null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking exam publication status: {ex.Message}");
                throw;
            }
        }
    }
}
