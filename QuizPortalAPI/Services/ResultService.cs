using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Result;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class ResultService : IResultService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResultService> _logger;

        public ResultService(AppDbContext context, ILogger<ResultService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get result by ID
        /// </summary>
        public async Task<ResultDTO?> GetResultByIdAsync(int resultId)
        {
            try
            {
                var result = await _context.Results
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .FirstOrDefaultAsync(r => r.ResultID == resultId);

                if (result == null)
                {
                    _logger.LogWarning($"Result {resultId} not found");
                    return null;
                }

                return MapToResultDTO(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving result {resultId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get student's result for a specific exam
        /// </summary>
        public async Task<ResultDTO?> GetStudentExamResultAsync(int examId, int studentId)
        {
            try
            {
                var result = await _context.Results
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

                if (result == null)
                {
                    _logger.LogWarning($"Result not found for exam {examId}, student {studentId}");
                    return null;
                }

                return MapToResultDTO(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam result: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all results for a student (paginated)
        /// </summary>
        public async Task<List<ResultDTO>> GetStudentResultsAsync(int studentId, int page, int pageSize)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                var results = await _context.Results
                    .Where(r => r.StudentID == studentId)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {results.Count} results for student {studentId} (page {page})");
                return results.Select(r => MapToResultDTO(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student results: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all results for a student (legacy, no pagination)
        /// </summary>
        public async Task<IEnumerable<ResultDTO>> GetStudentResultsLegacyAsync(int studentId)
        {
            try
            {
                var results = await _context.Results
                    .Where(r => r.StudentID == studentId)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {results.Count} results for student {studentId}");
                return results.Select(r => MapToResultDTO(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student results: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all results for an exam
        /// </summary>
        public async Task<ExamResultsDTO> GetExamResultsAsync(int examId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view results for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view results for your own exams");
                }

                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.TotalMarks)
                    .ToListAsync();

                var resultDTOs = results.Select(r => MapToResultDTO(r)).ToList();

                // ✅ FIX: Explicit cast from double to decimal
                var stats = new ExamResultsDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalSubmissions = results.Count,
                    GradedSubmissions = results.Count(r => r.Status == "Graded"),
                    AverageMarks = results.Count > 0 ? (decimal)results.Average(r => (double)r.TotalMarks) : 0,
                    HighestMarks = results.Count > 0 ? results.Max(r => r.TotalMarks) : 0,
                    LowestMarks = results.Count > 0 ? results.Min(r => r.TotalMarks) : 0,
                    Results = resultDTOs
                };

                _logger.LogInformation($"Retrieved results for exam {examId} by teacher {teacherId}");
                return stats;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam results: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calculate exam result for a student
        /// </summary>
        public async Task<ResultDTO> CalculateExamResultAsync(int examId, int studentId)
        {
            try
            {
                // ✅ Validate exam and student
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var student = await _context.Users.FindAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");

                // ✅ Get or create result
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

                // ✅ Calculate total marks from responses
                var totalMarks = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .SumAsync(sr => sr.MarksObtained);

                var examTotalMarks = await GetExamTotalMarksAsync(examId);

                result.TotalMarks = totalMarks;
                result.Percentage = examTotalMarks > 0 ? (totalMarks / examTotalMarks) * 100 : 0;
                result.UpdatedAt = DateTime.UtcNow;

                // ✅ Calculate rank
                result.Rank = await GetStudentRankAsync(examId, studentId);

                _context.Results.Update(result);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Result calculated for student {studentId} in exam {examId}");
                return MapToResultDTO(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating result: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Grade a subjective response
        /// </summary>
        public async Task<bool> GradeSubjectiveResponseAsync(int responseId, int evaluatorId, decimal marksObtained)
        {
            try
            {
                // ✅ Validate response exists
                var response = await _context.StudentResponses.FindAsync(responseId);
                if (response == null)
                    throw new InvalidOperationException("Response not found");

                // ✅ Validate question is subjective or SAQ
                var question = await _context.Questions.FindAsync(response.QuestionID);
                if (question == null)
                    throw new InvalidOperationException("Question not found");

                if (question.QuestionType == QuestionType.MCQ)
                    throw new InvalidOperationException("Cannot manually grade MCQ responses");

                // ✅ Validate marks
                if (marksObtained < 0 || marksObtained > question.Marks)
                    throw new ArgumentException($"Marks must be between 0 and {question.Marks}");

                // ✅ Update response
                response.MarksObtained = marksObtained;
                response.IsCorrect = marksObtained > 0;

                _context.StudentResponses.Update(response);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Response {responseId} graded by evaluator {evaluatorId} with {marksObtained} marks");
                return true;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error grading response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Publish result for student
        /// </summary>
        public async Task<bool> PublishResultAsync(int resultId, int teacherId)
        {
            try
            {
                var result = await _context.Results
                    .Include(r => r.Exam)
                    .FirstOrDefaultAsync(r => r.ResultID == resultId);

                if (result == null)
                    throw new InvalidOperationException("Result not found");

                // ✅ Verify teacher owns the exam
                if (result.Exam?.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to publish result they don't own");
                    throw new UnauthorizedAccessException("You can only publish results for your own exams");
                }

                result.Status = "Graded";
                result.EvaluatedBy = teacherId;
                result.EvaluatedAt = DateTime.UtcNow;
                result.UpdatedAt = DateTime.UtcNow;

                _context.Results.Update(result);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Result {resultId} published by teacher {teacherId}");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing result: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get exam's total marks
        /// </summary>
        public async Task<decimal> GetExamTotalMarksAsync(int examId)
        {
            try
            {
                return await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .SumAsync(q => q.Marks);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exam total marks: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get all results for an exam with pagination
        /// </summary>
        public async Task<List<ResultDTO>> GetExamAllResultsAsync(int examId, int teacherId, int page, int pageSize)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view results for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view results for your own exams");
                }

                var skip = (page - 1) * pageSize;

                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.TotalMarks)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"Teacher {teacherId} retrieved results for exam {examId} (page {page})");
                return results.Select(r => MapToResultDTO(r)).ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam results: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a specific student's result in an exam for teacher
        /// </summary>
        public async Task<ResultDTO?> GetStudentExamResultForTeacherAsync(int examId, int studentId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view result for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view results for your own exams");
                }

                var result = await _context.Results
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

                if (result == null)
                {
                    _logger.LogWarning($"Result not found for exam {examId}, student {studentId}");
                    return null;
                }

                _logger.LogInformation($"Teacher {teacherId} retrieved result for student {studentId} in exam {examId}");
                return MapToResultDTO(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student result: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get detailed exam result with question-wise breakdown
        /// </summary>
        public async Task<dynamic> GetExamResultDetailsAsync(int examId, int studentId)
        {
            try
            {
                var result = await _context.Results
                    .Include(r => r.Exam)
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

                if (result == null)
                    throw new InvalidOperationException("Result not found");

                var exam = await _context.Exams.FindAsync(examId);
                var totalMarks = await GetExamTotalMarksAsync(examId);

                var responses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Question!.Options)
                    .ToListAsync();

                var questionResults = responses.Select(sr => new
                {
                    sr.QuestionID,
                    QuestionText = sr.Question?.QuestionText ?? "N/A",
                    QuestionType = sr.Question?.QuestionType.ToString() ?? "Unknown",
                    IsAnswered = !string.IsNullOrEmpty(sr.AnswerText),
                    IsCorrect = sr.IsCorrect ?? false,
                    StudentAnswer = sr.AnswerText,
                    CorrectAnswer = sr.Question?.QuestionType == QuestionType.MCQ 
                        ? sr.Question.Options?.FirstOrDefault(o => o.IsCorrect)?.OptionText 
                        : "N/A",
                    MaxMarks = sr.Question?.Marks ?? 0,
                    MarksObtained = sr.MarksObtained
                }).ToList();

                return new
                {
                    ResultID = result.ResultID,
                    ExamID = examId,
                    ExamName = result.Exam?.Title ?? "Unknown",
                    TotalMarks = result.TotalMarks,
                    ExamTotalMarks = totalMarks,
                    Percentage = result.Percentage,
                    Status = result.Status,
                    TotalQuestions = responses.Count,
                    CorrectAnswers = responses.Count(r => r.IsCorrect == true),
                    UnansweredCount = responses.Count(r => string.IsNullOrEmpty(r.AnswerText)),
                    QuestionResults = questionResults
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving result details: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get result statistics for an exam
        /// </summary>
        public async Task<dynamic> GetResultStatisticsAsync(int examId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view statistics for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view statistics for your own exams");
                }

                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .ToListAsync();

                var totalMarks = await GetExamTotalMarksAsync(examId);

                return new
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalSubmissions = results.Count,
                    GradedSubmissions = results.Count(r => r.Status == "Graded"),
                    PendingSubmissions = results.Count(r => r.Status == "Pending"),
                    CompletedSubmissions = results.Count(r => r.Status == "Completed"),
                    AverageMarks = results.Count > 0 ? Math.Round(results.Average(r => (double)r.TotalMarks), 2) : 0,
                    HighestMarks = results.Count > 0 ? results.Max(r => r.TotalMarks) : 0,
                    LowestMarks = results.Count > 0 ? results.Min(r => r.TotalMarks) : 0,
                    ExamTotalMarks = totalMarks,
                    AveragePercentage = results.Count > 0 ? Math.Round(results.Average(r => (double)r.Percentage), 2) : 0
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving statistics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get result summary with grade distribution
        /// </summary>
        public async Task<dynamic> GetResultSummaryAsync(int examId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view summary for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view summary for your own exams");
                }

                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .ToListAsync();

                // Need to get PassingMarks if it exists, otherwise use 50% of total marks as default
                var totalMarks = await GetExamTotalMarksAsync(examId);
                var passingMarks = totalMarks > 0 ? totalMarks * 0.5m : 0; // 50% default

                var passCount = results.Count(r => r.TotalMarks >= passingMarks);
                var failCount = results.Count - passCount;

                return new
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalResults = results.Count,
                    PassCount = passCount,
                    FailCount = failCount,
                    PassPercentage = results.Count > 0 ? Math.Round((decimal)passCount / results.Count * 100, 2) : 0,
                    FailPercentage = results.Count > 0 ? Math.Round((decimal)failCount / results.Count * 100, 2) : 0,
                    GradeDistribution = new
                    {
                        APlusPlusCount = results.Count(r => r.Percentage >= 90),
                        APlusCount = results.Count(r => r.Percentage >= 80 && r.Percentage < 90),
                        BCount = results.Count(r => r.Percentage >= 70 && r.Percentage < 80),
                        CCount = results.Count(r => r.Percentage >= 60 && r.Percentage < 70),
                        DCount = results.Count(r => r.Percentage >= 50 && r.Percentage < 60),
                        FCount = results.Count(r => r.Percentage < 50)
                    }
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving summary: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get pass/fail breakdown for an exam
        /// </summary>
        public async Task<dynamic> GetPassFailBreakdownAsync(int examId, int teacherId)
        {
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view pass/fail for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view pass/fail data for your own exams");
                }

                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .Include(r => r.Student)
                    .ToListAsync();

                // Calculate passing marks as 50% of total exam marks (default)
                var totalMarks = await GetExamTotalMarksAsync(examId);
                var passingMarks = totalMarks > 0 ? totalMarks * 0.5m : 0;

                var passedStudents = results
                    .Where(r => r.TotalMarks >= passingMarks)
                    .Select(r => new
                    {
                        r.StudentID,
                        StudentName = r.Student?.FullName ?? "Unknown",
                        r.TotalMarks,
                        r.Percentage,
                        r.Rank
                    })
                    .ToList();

                var failedStudents = results
                    .Where(r => r.TotalMarks < passingMarks)
                    .Select(r => new
                    {
                        r.StudentID,
                        StudentName = r.Student?.FullName ?? "Unknown",
                        r.TotalMarks,
                        r.Percentage,
                        r.Rank
                    })
                    .ToList();

                return new
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    PassingMarks = passingMarks,
                    TotalResults = results.Count,
                    PassCount = passedStudents.Count,
                    FailCount = failedStudents.Count,
                    PassedStudents = passedStudents,
                    FailedStudents = failedStudents
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving pass/fail breakdown: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get student's total marks in an exam
        /// </summary>
        public async Task<decimal> GetStudentTotalMarksAsync(int examId, int studentId)
        {
            try
            {
                return await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .SumAsync(sr => sr.MarksObtained);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student total marks: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get student's rank in exam
        /// </summary>
        public async Task<int> GetStudentRankAsync(int examId, int studentId)
        {
            try
            {
                var studentMarks = await GetStudentTotalMarksAsync(examId, studentId);

                var rank = await _context.Results
                    .Where(r => r.ExamID == examId && r.TotalMarks > studentMarks)
                    .CountAsync();

                return rank + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating rank: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Check if result exists
        /// </summary>
        public async Task<bool> ResultExistsAsync(int examId, int studentId)
        {
            try
            {
                return await _context.Results
                    .AnyAsync(r => r.ExamID == examId && r.StudentID == studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking result existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Auto-grade MCQ responses for an exam
        /// </summary>
        public async Task<int> AutoGradeMCQResponsesAsync(int examId)
        {
            try
            {
                var mcqResponses = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Where(sr => sr.ExamID == examId && sr.Question!.QuestionType == QuestionType.MCQ)
                    .ToListAsync();

                int gradedCount = 0;

                foreach (var response in mcqResponses)
                {
                    if (response.Question == null)
                        continue;

                    // Get the correct answer option
                    var correctOption = await _context.QuestionOptions
                        .FirstOrDefaultAsync(o => o.QuestionID == response.QuestionID && o.IsCorrect);

                    if (correctOption != null)
                    {
                        bool isCorrect = response.AnswerText == correctOption.OptionID.ToString() || 
                                       response.AnswerText == correctOption.OptionText;

                        response.IsCorrect = isCorrect;
                        response.MarksObtained = isCorrect ? response.Question.Marks : 0;

                        _context.StudentResponses.Update(response);
                        gradedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Auto-graded {gradedCount} MCQ responses for exam {examId}");
                return gradedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error auto-grading MCQ responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Publish all results for an exam (bulk operation)
        /// </summary>
        public async Task<dynamic> PublishExamResultsAsync(int examId, int teacherId, decimal passingPercentage = 50)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ Verify teacher owns the exam
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to publish results for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only publish results for your own exams");
                }

                // ✅ Check if already published
                var alreadyPublished = await _context.Results
                    .Where(r => r.ExamID == examId && r.Status == "Graded")
                    .CountAsync();

                if (alreadyPublished > 0)
                    throw new InvalidOperationException($"Results already published for this exam. {alreadyPublished} results are in Graded status");

                // ✅ Auto-grade MCQ responses first
                await AutoGradeMCQResponsesAsync(examId);

                // ✅ Get all students who took the exam
                var studentIds = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .Select(sr => sr.StudentID)
                    .Distinct()
                    .ToListAsync();

                var totalExamMarks = await GetExamTotalMarksAsync(examId);
                var passingMarks = (totalExamMarks * passingPercentage) / 100;

                int resultsPublished = 0;
                var publishedResults = new List<dynamic>();

                foreach (var studentId in studentIds)
                {
                    // ✅ Calculate total marks for this student
                    var totalMarks = await GetStudentTotalMarksAsync(examId, studentId);

                    // ✅ Calculate percentage
                    var percentage = totalExamMarks > 0 ? (totalMarks / totalExamMarks) * 100 : 0;

                    // ✅ Determine pass/fail
                    var isPassed = totalMarks >= passingMarks;

                    // ✅ Calculate rank
                    var rank = await GetStudentRankAsync(examId, studentId);

                    // ✅ Get or create result
                    var result = await _context.Results
                        .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

                    bool isNewResult = false;
                    if (result == null)
                    {
                        result = new Result
                        {
                            ExamID = examId,
                            StudentID = studentId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Results.Add(result);
                        isNewResult = true;
                    }

                    // ✅ Update result with calculated values
                    result.TotalMarks = totalMarks;
                    result.Percentage = percentage;
                    result.Rank = rank;
                    result.Status = "Graded"; // Mark as published
                    result.EvaluatedBy = teacherId;
                    result.EvaluatedAt = DateTime.UtcNow;
                    result.UpdatedAt = DateTime.UtcNow;

                    // Only call Update() on existing results, newly added ones don't need it
                    if (!isNewResult)
                    {
                        _context.Results.Update(result);
                    }

                    resultsPublished++;

                    // ✅ Add to response list
                    publishedResults.Add(new
                    {
                        result.ResultID,
                        result.StudentID,
                        StudentName = result.Student?.FullName ?? "Unknown",
                        TotalMarks = totalMarks,
                        Percentage = Math.Round(percentage, 2),
                        Rank = rank,
                        Status = isPassed ? "Passed" : "Failed",
                        IsPassed = isPassed
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Published results for {resultsPublished} students in exam {examId} by teacher {teacherId}");

                return new
                {
                    success = true,
                    examID = examId,
                    examName = exam.Title,
                    totalExamMarks = totalExamMarks,
                    passingMarks = Math.Round(passingMarks, 2),
                    passingPercentage = passingPercentage,
                    resultsPublished = resultsPublished,
                    publishedAt = DateTime.UtcNow,
                    publishedBy = teacherId,
                    results = publishedResults
                };
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error publishing exam results: {ex.Message}");
                throw;
            }
        }

        private ResultDTO MapToResultDTO(Result result)
        {
            return new ResultDTO
            {
                ResultID = result.ResultID,
                ExamID = result.ExamID,
                ExamName = result.Exam?.Title ?? "Unknown",
                StudentID = result.StudentID,
                StudentName = result.Student?.FullName ?? "Unknown",
                StudentEmail = result.Student?.Email ?? "Unknown",
                TotalMarks = result.TotalMarks,
                ExamTotalMarks = 0, // Will be set by caller
                Rank = result.Rank,
                Percentage = result.Percentage,
                Status = result.Status,
                IsPublished = result.IsPublished,
                EvaluatorName = result.EvaluatorUser?.FullName,
                EvaluatedAt = result.EvaluatedAt,
                PublishedAt = result.PublishedAt,
                CreatedAt = result.CreatedAt
            };
        }

        // ============ EXAM PUBLICATION METHODS ============

        /// <summary>
        /// Check if all responses for an exam are graded
        /// </summary>
        public async Task<bool> AreAllResponsesGradedAsync(int examId)
        {
            try
            {
                var totalResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .CountAsync();

                if (totalResponses == 0)
                    return false; // No responses to grade

                var gradedResponses = await _context.GradingRecords
                    .Include(gr => gr.StudentResponse)
                    .Where(gr => gr.StudentResponse != null && gr.StudentResponse.ExamID == examId)
                    .Select(gr => gr.ResponseID)
                    .Distinct()
                    .CountAsync();

                var allGraded = totalResponses == gradedResponses;
                _logger.LogInformation($"Exam {examId}: {gradedResponses}/{totalResponses} responses graded. AllGraded: {allGraded}");

                return allGraded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking grading status for exam {examId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get grading progress for an exam
        /// </summary>
        public async Task<dynamic> GetGradingProgressAsync(int examId, int teacherId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                    throw new UnauthorizedAccessException("You can only view progress for your own exams");

                var totalStudents = await _context.StudentResponses
                    .Where(r => r.ExamID == examId)
                    .CountAsync();

                var gradedStudents = await _context.Results
                    .Where(r => r.ExamID == examId && r.Status == "Graded")
                    .CountAsync();

                var pendingStudents = totalStudents - gradedStudents;

                var progress = totalStudents > 0 ? (gradedStudents * 100m) / totalStudents : 0;

                return new
                {
                    examID = examId,
                    examTitle = exam.Title,
                    totalStudents = totalStudents,
                    gradedStudents = gradedStudents,
                    pendingStudents = pendingStudents,
                    gradingProgress = Math.Round(progress, 2),
                    allGraded = pendingStudents == 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting grading progress: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Publish an exam - marks all results as published and creates ExamPublication record
        /// Only possible if all responses are graded
        /// </summary>
        public async Task<dynamic> PublishExamAsync(int examId, int teacherId, decimal passingPercentage = 50, string? notes = null)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ Verify exam exists and teacher owns it
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to publish exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only publish results for your own exams");
                }

                // ✅ Check if exam is already published
                var existingPublication = await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId);

                if (existingPublication != null && existingPublication.Status == "Published")
                {
                    throw new InvalidOperationException("This exam has already been published");
                }

                // ✅ Validate passing percentage
                if (passingPercentage < 0 || passingPercentage > 100)
                    throw new ArgumentException("Passing percentage must be between 0 and 100");

                // ✅ Get all results for the exam
                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .ToListAsync();

                if (results.Count == 0)
                    throw new InvalidOperationException("No student results found for this exam");

                // ✅ Check if all responses are graded
                var allGraded = await AreAllResponsesGradedAsync(examId);
                if (!allGraded)
                {
                    var totalResponses = await _context.StudentResponses
                        .Where(sr => sr.ExamID == examId)
                        .CountAsync();

                    var gradedResponses = await _context.GradingRecords
                        .Include(gr => gr.StudentResponse)
                        .Where(gr => gr.StudentResponse != null && gr.StudentResponse.ExamID == examId)
                        .Select(gr => gr.ResponseID)
                        .Distinct()
                        .CountAsync();

                    var pendingCount = totalResponses - gradedResponses;

                    throw new InvalidOperationException(
                        $"Cannot publish exam. {pendingCount} out of {totalResponses} responses are still pending grading");
                }

                // ✅ Get grading progress
                var gradedStudents = results.Count(r => r.Status == "Graded");
                var totalStudents = results.Count;

                // ✅ Mark all results as published
                var publishedCount = 0;
                foreach (var result in results)
                {
                    result.IsPublished = true;
                    result.PublishedAt = DateTime.UtcNow;
                    _context.Results.Update(result);
                    publishedCount++;
                }

                // ✅ Create or update ExamPublication record
                if (existingPublication == null)
                {
                    existingPublication = new ExamPublication
                    {
                        ExamID = examId,
                        Status = "Published",
                        TotalStudents = totalStudents,
                        GradedStudents = gradedStudents,
                        PassingPercentage = passingPercentage,
                        PublishedBy = teacherId,
                        PublishedAt = DateTime.UtcNow,
                        PublicationNotes = notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ExamPublications.Add(existingPublication);
                }
                else
                {
                    existingPublication.Status = "Published";
                    existingPublication.TotalStudents = totalStudents;
                    existingPublication.GradedStudents = gradedStudents;
                    existingPublication.PassingPercentage = passingPercentage;
                    existingPublication.PublishedBy = teacherId;
                    existingPublication.PublishedAt = DateTime.UtcNow;
                    existingPublication.PublicationNotes = notes;
                    existingPublication.UpdatedAt = DateTime.UtcNow;
                    _context.ExamPublications.Update(existingPublication);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Published exam {examId} with {publishedCount} results by teacher {teacherId}");

                return new
                {
                    success = true,
                    message = $"Exam published successfully. {publishedCount} results are now visible to students.",
                    examID = examId,
                    examTitle = exam.Title,
                    totalStudents = totalStudents,
                    gradedStudents = gradedStudents,
                    passingPercentage = passingPercentage,
                    publishedAt = DateTime.UtcNow,
                    publishedBy = teacherId,
                    resultsPublished = publishedCount
                };
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation during exam publication: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized access during exam publication: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error publishing exam {examId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get exam publication status
        /// </summary>
        public async Task<dynamic?> GetExamPublicationStatusAsync(int examId, int teacherId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                    throw new UnauthorizedAccessException("You can only view publication status for your own exams");

                var publication = await _context.ExamPublications
                    .Include(ep => ep.PublishedByUser)
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId);

                if (publication == null)
                {
                    // Exam not yet published, return grading progress
                    var gradingProgress = await GetGradingProgressAsync(examId, teacherId);
                    return new
                    {
                        isPublished = false,
                        examID = examId,
                        examTitle = exam.Title,
                        gradingProgress = gradingProgress
                    };
                }

                return new
                {
                    isPublished = publication.Status == "Published",
                    publicationID = publication.PublicationID,
                    examID = examId,
                    examTitle = exam.Title,
                    status = publication.Status,
                    totalStudents = publication.TotalStudents,
                    gradedStudents = publication.GradedStudents,
                    passingPercentage = publication.PassingPercentage,
                    publishedBy = publication.PublishedByUser?.FullName,
                    publishedAt = publication.PublishedAt,
                    publicationNotes = publication.PublicationNotes,
                    createdAt = publication.CreatedAt,
                    updatedAt = publication.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting publication status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if exam is published
        /// </summary>
        public async Task<bool> IsExamPublishedAsync(int examId)
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

        /// <summary>
        /// Get all published exam results for a student
        /// </summary>
        public async Task<List<ResultDTO>> GetPublishedExamResultsAsync(int studentId)
        {
            try
            {
                var results = await _context.Results
                    .Where(r => r.StudentID == studentId && r.IsPublished)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.PublishedAt)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {results.Count} published results for student {studentId}");
                return results.Select(r => MapToResultDTO(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving published results: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unpublish an exam (reverses publication)
        /// </summary>
        public async Task<dynamic> UnpublishExamAsync(int examId, int teacherId, string? reason = null)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to unpublish exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only unpublish exams you own");
                }

                var publication = await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId);

                if (publication == null || publication.Status != "Published")
                    throw new InvalidOperationException("This exam is not published");

                // ✅ Mark all results as unpublished
                var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .ToListAsync();

                foreach (var result in results)
                {
                    result.IsPublished = false;
                    _context.Results.Update(result);
                }

                // ✅ Update publication record
                publication.Status = "NotPublished";
                publication.PublishedAt = null;
                publication.PublishedBy = null;
                publication.PublicationNotes = reason;
                publication.UpdatedAt = DateTime.UtcNow;
                _context.ExamPublications.Update(publication);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Unpublished exam {examId} by teacher {teacherId}");

                return new
                {
                    success = true,
                    message = "Exam unpublished successfully. Results are no longer visible to students.",
                    examID = examId,
                    examTitle = exam.Title,
                    resultsUnpublished = results.Count,
                    unpublishedAt = DateTime.UtcNow,
                    reason = reason
                };
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Invalid operation during exam unpublishing: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Unauthorized access during exam unpublishing: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error unpublishing exam {examId}: {ex.Message}");
                throw;
            }
        }
    }
}