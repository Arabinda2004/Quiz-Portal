using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.DAL.ExamPublicationRepo;
using QuizPortalAPI.DAL.ResultRepo;
using QuizPortalAPI.DAL.StudentResponseRepo;
using QuizPortalAPI.DAL.GradingRecordRepo;
using QuizPortalAPI.DAL.UserRepo;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Grading;
using QuizPortalAPI.Models;
using QuizPortalAPI.DAL.QuestionRepo;
using QuizPortalAPI.DAL.ExamRepo;

namespace QuizPortalAPI.Services
{
    public class GradingService : IGradingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GradingService> _logger;
        private readonly IExamService _examService;
        private readonly IExamPublicationRepository _examPublicationRepository;
        private readonly IResultRepository _resultRepository;
        private readonly IStudentResponseRepository _studentResponseRepository;
        private readonly IGradingRecordRepository _gradingRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExamRepository _examRepository;

        public GradingService(AppDbContext context, ILogger<GradingService> logger, IExamService examService, IExamPublicationRepository examPublicationRepository, IResultRepository resultRepository, IStudentResponseRepository studentResponseRepository, IGradingRecordRepository gradingRepository, IQuestionRepository questionRepository, IExamRepository examRepository, IUserRepository userRepository)
        {
            _context = context;
            _logger = logger;
            _examService = examService;
            _examPublicationRepository = examPublicationRepository;
            _resultRepository = resultRepository;
            _studentResponseRepository = studentResponseRepository;
            _gradingRepository = gradingRepository;
            _questionRepository = questionRepository;
            _examRepository = examRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all pending responses for an exam
        /// </summary>
        public async Task<PendingResponsesDTO> GetPendingResponsesAsync(int examId, int teacherId, int page = 1, int pageSize = 10)
        {
            try
            {
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var allResponses = await _studentResponseRepository.GetAllSubmittedResponsesByExamIdAsync(examId);

                var gradingRecords = await _gradingRepository.GetAllGradingRecordsFromSubmittedResponsesAsync(allResponses); // give me all records that belong to the responses in 'allResponses' and that are graded

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
        /// Get pending responses for a specific student
        /// </summary>
        public async Task<PendingResponsesDTO> GetPendingResponsesByStudentAsync(int examId, int studentId, int teacherId, int page = 1, int pageSize = 10)
        {
            try
            {
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view responses for your own exams");

                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (DateTime.UtcNow < exam.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var student = await _userRepository.GetUserDetailsByIdAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");

                var pendingResponses = await _studentResponseRepository.GetPendingResponsesAsync(examId, studentId);

                var allResponses = await _studentResponseRepository.GetAllStudentResponsesAsync(examId, studentId);

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
                var response = await _studentResponseRepository.GetStudentResponseIncludingQuestionExamAndStudentByResponseIdAsync(responseId);

                if (response == null)
                {
                    _logger.LogWarning($"Response {responseId} not found");
                    return null;
                }

                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                var gradingRecord = await _gradingRepository.GetGradingRecordWithEvaluatorByResponseIdAsync(responseId);

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
                var response = await _studentResponseRepository.GetstudentResponseByResponseIdAsync(responseId);

                if (response == null)
                    throw new InvalidOperationException("Response not found");

                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only grade responses for your own exams");

                if (DateTime.UtcNow < response.Exam!.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {response.Exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var isPublished = await IsExamPublishedAsync(response.ExamID);
                if (isPublished)
                    throw new InvalidOperationException("Cannot update marks for a published exam. Please unpublish the exam first to make changes.");

                if (gradeDto.MarksObtained > response.Question?.Marks)
                    throw new ArgumentException($"Marks cannot exceed {response.Question?.Marks}");

                var oldGrading = await _gradingRepository.GetGradingRecordUsingResponseIdAsync(responseId);
                // if there was an old grading, remove it
                if (oldGrading != null)
                {
                    await _gradingRepository.RemoveAsync(oldGrading);
                    await _gradingRepository.SaveChangesAsync();
                }

                response.MarksObtained = gradeDto.MarksObtained;
                response.IsCorrect = gradeDto.MarksObtained > 0;

                await _studentResponseRepository.UpdateAsync(response);
                await _studentResponseRepository.SaveChangesAsync();

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

                await _gradingRepository.AddAsync(gradingRecord);
                await _gradingRepository.SaveChangesAsync();

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
        /// Get grading statistics for an exam
        /// </summary>
        public async Task<GradingStatsDTO> GetGradingStatsAsync(int examId, int teacherId)
        {
            try
            {
                var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only view statistics for your own exams");

                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var questions = await _questionRepository.GetExamQuestionsByExamIdAsync(examId);

                var questionsWithResponses = new List<QuestionGradingStatsDTO>();
                int totalResponses = 0;
                int totalGraded = 0;

                foreach (var question in questions)
                {
                    var responses = await _studentResponseRepository.GetStudentResponseForAQuestionByIdAsync(question.QuestionID);

                    var gradedCount = await _gradingRepository.GetGradingRecordsBasedOnQuestionIdAsync(question.QuestionID);

                    var averageMarks = gradedCount > 0
                        ? await _gradingRepository.GetAverageMarksByQuestionIdAsync(question.QuestionID)
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

                var uniqueStudentsResponded = await _studentResponseRepository.GetUniqueStudentResponseCountAsync(examId);

                var studentsFullyGraded = 0;
                if (uniqueStudentsResponded > 0)
                {
                    var allStudentResponses = await _studentResponseRepository.GetAllResponsesOfAnExamByIdAsync(examId);

                    var studentGradingCounts = allStudentResponses
                        .GroupBy(sr => sr.StudentID)
                        .ToDictionary(g => g.Key, g => g.Count());
                        // (group by -> will be the key) Here, studentID is the key

                    foreach (var studentId in studentGradingCounts.Keys)
                    {
                        var totalQuestionsForStudent = studentGradingCounts[studentId];
                        var gradedQuestionsForStudent = await _gradingRepository.GetGradedQuestionsForStudentAsync(studentId, examId);

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
                    TotalStudents = await _resultRepository.GetDistinctStudentResultCountAsync(examId),
                    TotalResponses = uniqueStudentsResponded,
                    GradedResponses = studentsFullyGraded,
                    PendingResponses = uniqueStudentsResponded - studentsFullyGraded,
                    GradingPercentage = uniqueStudentsResponded > 0 ? (decimal)studentsFullyGraded / uniqueStudentsResponded * 100 : 0,
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

        // /// <summary>
        // /// Mark a question as fully graded
        // /// </summary>
        // public async Task<bool> MarkQuestionGradedAsync(int examId, int questionId, int teacherId)
        // {
        //     try
        //     {
        //         // ✅ Verify teacher owns the exam
        //         var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
        //         if (!isOwner)
        //             throw new UnauthorizedAccessException("You can only grade responses for your own exams");

        //         var question = await _context.Questions.FindAsync(questionId);
        //         if (question == null || question.ExamID != examId)
        //             throw new InvalidOperationException("Question not found in this exam");

        //         // Check if all responses for this question are graded
        //         var totalResponses = await _context.StudentResponses
        //             .CountAsync(sr => sr.QuestionID == questionId);

        //         var gradedResponses = await _context.GradingRecords
        //             .CountAsync(gr => gr.QuestionID == questionId && gr.Status == "Graded");

        //         if (totalResponses != gradedResponses)
        //             throw new InvalidOperationException("Cannot mark question as graded. Some responses are still pending.");

        //         _logger.LogInformation($"Question {questionId} marked as graded by teacher {teacherId}");
        //         return true;
        //     }
        //     catch (UnauthorizedAccessException ex)
        //     {
        //         _logger.LogWarning($"Unauthorized: {ex.Message}");
        //         throw;
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         _logger.LogWarning($"Invalid operation: {ex.Message}");
        //         throw;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"Error marking question as graded: {ex.Message}");
        //         throw;
        //     }
        // }

        // /// <summary>
        // /// Get all graded responses for a student
        // /// </summary>
        // public async Task<StudentGradedResponsesDTO> GetStudentGradedResponsesAsync(int examId, int studentId, int teacherId)
        // {
        //     try
        //     {
        //         // ✅ Verify teacher owns the exam
        //         var isOwner = await _examService.IsTeacherExamOwnerAsync(examId, teacherId);
        //         if (!isOwner)
        //             throw new UnauthorizedAccessException("You can only view responses for your own exams");

        //         var exam = await _context.Exams.FindAsync(examId);
        //         if (exam == null)
        //             throw new InvalidOperationException("Exam not found");

        //         var student = await _context.Users.FindAsync(studentId);
        //         if (student == null)
        //             throw new InvalidOperationException("Student not found");

        //         // Get all responses for this student
        //         var responses = await _context.StudentResponses
        //             .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
        //             .Include(sr => sr.Question)
        //             .ToListAsync();

        //         var gradedResponses = new List<GradedResponseItemDTO>();
        //         decimal totalMarksObtained = 0;
        //         decimal totalMaxMarks = 0;

        //         foreach (var response in responses)
        //         {
        //             var gradingRecord = await _context.GradingRecords
        //                 .Include(gr => gr.GradedByTeacher)
        //                 .FirstOrDefaultAsync(gr => gr.ResponseID == response.ResponseID && gr.Status == "Graded");

        //             if (gradingRecord != null)
        //             {
        //                 var marksObtained = gradingRecord.MarksObtained;
        //                 var maxMarks = response.Question?.Marks ?? 0;

        //                 totalMarksObtained += marksObtained;
        //                 totalMaxMarks += maxMarks;

        //                 gradedResponses.Add(new GradedResponseItemDTO
        //                 {
        //                     ResponseID = response.ResponseID,
        //                     QuestionID = response.QuestionID,
        //                     QuestionText = response.Question?.QuestionText ?? "Unknown",
        //                     QuestionType = response.Question?.QuestionType.ToString() ?? "Unknown",
        //                     MaxMarks = maxMarks,
        //                     MarksObtained = marksObtained,
        //                     StudentAnswer = response.AnswerText,
        //                     Feedback = gradingRecord.Feedback,
        //                     Comment = gradingRecord.Comment,
        //                     GradedAt = gradingRecord.GradedAt,
        //                     GradedBy = gradingRecord.GradedByTeacher?.FullName ?? "Unknown"
        //                 });
        //             }
        //         }

        //         return new StudentGradedResponsesDTO
        //         {
        //             StudentID = studentId,
        //             StudentName = student.FullName,
        //             ExamID = examId,
        //             ExamName = exam.Title,
        //             TotalMarksObtained = totalMarksObtained,
        //             TotalMaxMarks = totalMaxMarks,
        //             Percentage = totalMaxMarks > 0 ? (totalMarksObtained / totalMaxMarks) * 100 : 0,
        //             TotalQuestions = responses.Count,
        //             GradedResponses = gradedResponses
        //         };
        //     }
        //     catch (UnauthorizedAccessException ex)
        //     {
        //         _logger.LogWarning($"Unauthorized: {ex.Message}");
        //         throw;
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         _logger.LogWarning($"Invalid operation: {ex.Message}");
        //         throw;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"Error getting student graded responses: {ex.Message}");
        //         throw;
        //     }
        // }

        /// <summary>
        /// Regrade a response with reason tracking
        /// </summary>
        public async Task<bool> RegradeResponseAsync(int responseId, int teacherId, RegradingDTO regradingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var response = await _studentResponseRepository.GetstudentResponseByResponseIdAsync(responseId);

                if (response == null)
                    throw new InvalidOperationException("Response not found");

                var isOwner = await _examService.IsTeacherExamOwnerAsync(response.ExamID, teacherId);
                if (!isOwner)
                    throw new UnauthorizedAccessException("You can only regrade responses for your own exams");

                if (DateTime.UtcNow < response.Exam!.ScheduleEnd)
                    throw new InvalidOperationException($"Exam has not ended yet. Grading will be available after {response.Exam.ScheduleEnd:yyyy-MM-dd HH:mm:ss} UTC");

                var isPublished = await IsExamPublishedAsync(response.ExamID);
                if (isPublished)
                    throw new InvalidOperationException("Cannot regrade a published exam. Please unpublish the exam first to make changes.");

                var oldGrading = await _gradingRepository.GetGradingRecordUsingResponseIdAsync(responseId);

                if (oldGrading == null)
                    throw new InvalidOperationException("No grading record found to regrade");

                oldGrading.Status = "Regraded";
                await _gradingRepository.UpdateAsync(oldGrading);
                await _gradingRepository.SaveChangesAsync();

                if (regradingDto.NewMarksObtained > response.Question?.Marks)
                    throw new ArgumentException($"Marks cannot exceed {response.Question?.Marks}");

                response.MarksObtained = regradingDto.NewMarksObtained;
                response.IsCorrect = regradingDto.NewMarksObtained > 0;
                await _studentResponseRepository.UpdateAsync(response);
                await _studentResponseRepository.SaveChangesAsync();

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

                await _gradingRepository.AddAsync(newGrading);
                await _gradingRepository.SaveChangesAsync();

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
                var result = await _resultRepository.GetStudentResultByExamAndStudentIdAsync(examId, studentId);

                if (result == null)
                {
                    result = new Result
                    {
                        ExamID = examId,
                        StudentID = studentId,
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _resultRepository.AddAsync(result);
                }

                var studentResponses = await _studentResponseRepository.GetStudentResponsesOfAnExamByStudentIdAsync(examId, studentId);

                // Check how many responses are graded
                var gradedResponseIds = await _gradingRepository.CheckHowManyResponsesAreGradedAsync(studentResponses);

                // Check if all responses are graded
                var allResponsesGraded = studentResponses.Count > 0 && studentResponses.Count == gradedResponseIds.Count;

                // Calculate total marks
                var totalMarks = await _studentResponseRepository.GetStudentTotalMarksFromTheirResponseAsync(examId, studentId);

                // Get exam total marks
                var examTotalMarks = await _questionRepository.CalculateExamTotalMarksByExamIdAsync(examId);

                result.TotalMarks = totalMarks;
                result.Percentage = examTotalMarks > 0 ? (totalMarks / examTotalMarks) * 100 : 0;
                result.UpdatedAt = DateTime.UtcNow;
                
                // Update status based on grading completion
                result.Status = allResponsesGraded ? "Graded" : "Completed";

                // Calculate rank
                var rank = await _resultRepository.CalculateRankAsync(examId, totalMarks);
                result.Rank = rank + 1;

                await _resultRepository.UpdateAsync(result);
                await _resultRepository.SaveChangesAsync();

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
                var publication = await _examPublicationRepository.GetExamPublicationStatusByExamIdAsync(examId);

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
