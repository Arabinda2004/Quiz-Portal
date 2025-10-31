using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.StudentResponse;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class StudentResponseService : IStudentResponseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentResponseService> _logger;

        public StudentResponseService(AppDbContext context, ILogger<StudentResponseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Student submits an answer to a question
        /// </summary>
        public async Task<StudentResponseDTO> SubmitAnswerAsync(int examId, int studentId, CreateStudentResponseDTO createResponseDTO)
        {
            try
            {
                if (createResponseDTO == null)
                    throw new ArgumentNullException(nameof(createResponseDTO));

                // ✅ Validate exam exists and is active
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var now = DateTime.UtcNow;
                if (now < exam.ScheduleStart || now > exam.ScheduleEnd)
                    throw new InvalidOperationException("Exam is not active");

                // ✅ Check if exam has been published - prevent new submissions
                var publication = await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId && ep.Status == "Published");

                if (publication != null)
                    throw new InvalidOperationException("This exam has been published and no further submissions are allowed");

                // ✅ Validate question exists
                var question = await _context.Questions.FindAsync(createResponseDTO.QuestionID);
                if (question == null)
                    throw new InvalidOperationException("Question not found");

                if (question.ExamID != examId)
                    throw new InvalidOperationException("Question does not belong to this exam");

                // ✅ Validate student exists
                var student = await _context.Users.FindAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");

                // ✅ Check if response already exists
                var existingResponse = await _context.StudentResponses
                    .FirstOrDefaultAsync(sr => sr.ExamID == examId && 
                                              sr.QuestionID == createResponseDTO.QuestionID && 
                                              sr.StudentID == studentId);

                StudentResponse response;

                if (existingResponse != null)
                {
                    // ✅ Update existing response
                    response = existingResponse;
                    response.AnswerText = createResponseDTO.AnswerText;
                    response.SubmittedAt = DateTime.UtcNow;

                    // ✅ Re-grade MCQ responses on update
                    if (question.QuestionType == QuestionType.MCQ)
                    {
                        var correctOption = await _context.QuestionOptions
                            .FirstOrDefaultAsync(qo => qo.QuestionID == question.QuestionID && qo.IsCorrect);

                        if (correctOption != null && correctOption.OptionText == createResponseDTO.AnswerText)
                        {
                            response.IsCorrect = true;
                            response.MarksObtained = question.Marks;
                        }
                        else
                        {
                            response.IsCorrect = false;
                            response.MarksObtained = 0;
                        }
                    }

                    _context.StudentResponses.Update(response);
                }
                else
                {
                    // ✅ Create new response
                    response = new StudentResponse
                    {
                        ExamID = examId,
                        QuestionID = createResponseDTO.QuestionID,
                        StudentID = studentId,
                        AnswerText = createResponseDTO.AnswerText,
                        SubmittedAt = DateTime.UtcNow
                    };

                    // // ✅ Auto-grade MCQ responses
                    // if (question.QuestionType == QuestionType.MCQ)
                    // {
                    //     var correctOption = await _context.QuestionOptions
                    //         .FirstOrDefaultAsync(qo => qo.QuestionID == question.QuestionID && qo.IsCorrect);

                    //     if (correctOption != null && correctOption.OptionText == createResponseDTO.AnswerText)
                    //     {
                    //         response.IsCorrect = true;
                    //         response.MarksObtained = question.Marks;
                    //     }
                    //     else
                    //     {
                    //         response.IsCorrect = false;
                    //         response.MarksObtained = 0;
                    //     }
                    // }
                    // else
                    // {
                    //     // ✅ Subjective/SAQ responses need manual evaluation
                    //     response.IsCorrect = null;
                    //     response.MarksObtained = 0;
                    // }

                    response.IsCorrect = null;
                    response.MarksObtained = 0;

                    _context.StudentResponses.Add(response);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Response submitted by student {studentId} for question {createResponseDTO.QuestionID} in exam {examId}");
                return MapToStudentResponseDTO(response, question);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Validation error: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get response by ID
        /// </summary>
        public async Task<StudentResponseDTO?> GetResponseByIdAsync(int responseId)
        {
            try
            {
                var response = await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);

                if (response == null)
                {
                    _logger.LogWarning($"Response {responseId} not found");
                    return null;
                }

                return MapToStudentResponseDTO(response, response.Question!);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving response {responseId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all responses submitted by a student for an exam
        /// </summary>
        public async Task<StudentExamResponsesDTO> GetStudentExamResponsesAsync(int examId, int studentId)
        {
            try
            {
                // ✅ Validate exam exists
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Get all questions in the exam
                var questions = await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .ToListAsync();

                // ✅ Get student's responses with grading information
                var responses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Include(sr => sr.Question)
                    .ToListAsync();

                var responseDTOs = new List<StudentResponseDTO>();
                foreach (var response in responses)
                {
                    // Get grading record if exists to include feedback
                    var gradingRecord = await _context.GradingRecords
                        .FirstOrDefaultAsync(gr => gr.ResponseID == response.ResponseID && gr.Status == "Graded");

                    var dto = MapToStudentResponseDTO(response, response.Question!);
                    
                    // Set IsGraded flag based on whether a grading record exists
                    dto.IsGraded = gradingRecord != null;
                    
                    if (gradingRecord != null)
                    {
                        dto.Feedback = gradingRecord.Feedback;
                    }
                    responseDTOs.Add(dto);
                }

                var unanswered = questions.Count - responses.Count;

                var result = new StudentExamResponsesDTO
                {
                    ExamID = examId,
                    ExamName = exam.Title,
                    TotalQuestions = questions.Count,
                    AnsweredQuestions = responses.Count,
                    UnansweredQuestions = unanswered,
                    Responses = responseDTOs
                };

                _logger.LogInformation($"Retrieved {responses.Count} responses for student {studentId} in exam {examId}");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all student responses for a specific question
        /// </summary>
        public async Task<IEnumerable<StudentResponseDTO>> GetExamResponsesByQuestionAsync(int questionId)
        {
            try
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                    throw new InvalidOperationException("Question not found");

                var responses = await _context.StudentResponses
                    .Where(sr => sr.QuestionID == questionId)
                    .Include(sr => sr.Question)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {responses.Count} responses for question {questionId}");
                return responses.Select(r => MapToStudentResponseDTO(r, r.Question!)).ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if student can submit answer
        /// </summary>
        public async Task<bool> CanSubmitAnswerAsync(int examId, int studentId)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    return false;

                var now = DateTime.UtcNow;
                bool isExamActive = now >= exam.ScheduleStart && now <= exam.ScheduleEnd;
                
                // ✅ Check if exam is published - if published, students cannot submit new answers
                var isExamPublished = await _context.ExamPublications
                    .AnyAsync(ep => ep.ExamID == examId && ep.Status == "Published");

                return isExamActive && !isExamPublished;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking submission eligibility: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get count of responses submitted by student
        /// </summary>
        public async Task<int> GetStudentResponseCountAsync(int examId, int studentId)
        {
            try
            {
                return await _context.StudentResponses
                    .CountAsync(sr => sr.ExamID == examId && sr.StudentID == studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting response count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Check if response exists
        /// </summary>
        public async Task<bool> ResponseExistsAsync(int examId, int questionId, int studentId)
        {
            try
            {
                return await _context.StudentResponses
                    .AnyAsync(sr => sr.ExamID == examId && 
                                   sr.QuestionID == questionId && 
                                   sr.StudentID == studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking response existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Withdraw/delete a submitted response
        /// </summary>
        public async Task<bool> WithdrawResponseAsync(int responseId, int studentId)
        {
            try
            {
                var response = await _context.StudentResponses.FindAsync(responseId);
                if (response == null)
                    throw new InvalidOperationException("Response not found");

                // ✅ Verify student owns the response
                if (response.StudentID != studentId)
                    throw new UnauthorizedAccessException("You can only withdraw your own responses");

                // ✅ Check if exam is still active
                var exam = await _context.Exams.FindAsync(response.ExamID);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var now = DateTime.UtcNow;
                if (now > exam.ScheduleEnd)
                    throw new InvalidOperationException("Cannot withdraw response after exam ends");

                _context.StudentResponses.Remove(response);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Response {responseId} withdrawn by student {studentId}");
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
                _logger.LogError($"Error withdrawing response: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get question-wise student statistics
        /// </summary>
        public async Task<QuestionStatisticsDTO> GetQuestionStatisticsAsync(int questionId, int examId)
        {
            try
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question == null || question.ExamID != examId)
                    throw new InvalidOperationException("Question not found in this exam");

                var responses = await _context.StudentResponses
                    .Where(sr => sr.QuestionID == questionId)
                    .ToListAsync();

                var totalResponses = responses.Count;
                var correctResponses = responses.Count(r => r.IsCorrect == true);
                var incorrectResponses = responses.Count(r => r.IsCorrect == false);
                var unevaluatedResponses = responses.Count(r => r.IsCorrect == null);

                var stats = new QuestionStatisticsDTO
                {
                    QuestionID = questionId,
                    QuestionText = question.QuestionText,
                    TotalResponses = totalResponses,
                    CorrectResponses = correctResponses,
                    IncorrectResponses = incorrectResponses,
                    UnevaluatedResponses = unevaluatedResponses,
                    CorrectPercentage = totalResponses > 0 ? ((decimal)correctResponses / totalResponses) * 100 : 0,
                    AverageMarksObtained = totalResponses > 0 ? responses.Average(r => (double)r.MarksObtained) : 0
                };

                _logger.LogInformation($"Retrieved statistics for question {questionId}");
                return stats;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question statistics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all students who attempted an exam with their submission details
        /// </summary>
        public async Task<StudentAttemptsResponseDTO> GetAllStudentAttemptsAsync(int examId, int page, int pageSize)
        {
            try
            {
                // ✅ Validate exam exists
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Get all questions for this exam
                var totalQuestions = await _context.Questions
                    .CountAsync(q => q.ExamID == examId);

                // ✅ Get unique students who attempted this exam
                var studentAttempts = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .GroupBy(sr => sr.StudentID)
                    .Select(g => new { StudentId = g.Key, ResponseCount = g.Count() })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalCount = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .Select(sr => sr.StudentID)
                    .Distinct()
                    .CountAsync();

                var studentAttemptDTOs = new List<StudentAttemptDTO>();

                foreach (var attempt in studentAttempts)
                {
                    var student = await _context.Users.FindAsync(attempt.StudentId);
                    if (student == null) continue;

                    var responses = await _context.StudentResponses
                        .Where(sr => sr.ExamID == examId && sr.StudentID == attempt.StudentId)
                        .ToListAsync();

                    var firstResponse = responses.FirstOrDefault();
                    var lastResponse = responses.OrderByDescending(r => r.SubmittedAt).FirstOrDefault();

                    studentAttemptDTOs.Add(new StudentAttemptDTO
                    {
                        StudentId = student.UserID,
                        StudentName = student.FullName,
                        StudentEmail = student.Email,
                        TotalQuestions = totalQuestions,
                        AnsweredQuestions = responses.Count,
                        UnansweredQuestions = totalQuestions - responses.Count,
                        ProgressPercentage = totalQuestions > 0 ? Math.Round(((decimal)responses.Count / totalQuestions) * 100, 2) : 0,
                        AttemptedAt = firstResponse?.SubmittedAt ?? DateTime.UtcNow,
                        CompletedAt = lastResponse?.SubmittedAt,
                        Status = "Completed",
                        TimeSpentMinutes = lastResponse != null && firstResponse != null
                            ? (int)(lastResponse.SubmittedAt - firstResponse.SubmittedAt).TotalMinutes
                            : 0
                    });
                }

                _logger.LogInformation($"Retrieved {studentAttemptDTOs.Count} student attempts for exam {examId}");

                return new StudentAttemptsResponseDTO
                {
                    TotalCount = totalCount,
                    Students = studentAttemptDTOs
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student attempts: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all responses for a specific question with pagination (for teacher grading)
        /// </summary>
        public async Task<QuestionResponsesPagedDTO> GetExamResponsesByQuestionPagedAsync(int questionId, int examId, int page, int pageSize)
        {
            try
            {
                // ✅ Validate question exists and belongs to exam
                var question = await _context.Questions.FindAsync(questionId);
                if (question == null || question.ExamID != examId)
                    throw new InvalidOperationException("Question not found in this exam");

                // ✅ Get total count
                var totalCount = await _context.StudentResponses
                    .CountAsync(sr => sr.QuestionID == questionId);

                // ✅ Get paginated responses
                var responses = await _context.StudentResponses
                    .Where(sr => sr.QuestionID == questionId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(sr => sr.Question)
                    .ToListAsync();

                var responseDTOs = new List<QuestionResponseDTO>();

                foreach (var response in responses)
                {
                    var student = await _context.Users.FindAsync(response.StudentID);
                    if (student == null) continue;

                    responseDTOs.Add(new QuestionResponseDTO
                    {
                        ResponseId = response.ResponseID,
                        StudentId = student.UserID,
                        StudentName = student.FullName,
                        StudentEmail = student.Email,
                        ResponseText = response.AnswerText,
                        SelectedOptionText = response.AnswerText, // For MCQ
                        SubmissionTime = response.SubmittedAt,
                        IsGraded = response.IsCorrect.HasValue,
                        MarksAwarded = response.MarksObtained > 0 ? (int)response.MarksObtained : null,
                        Feedback = null,
                        GradingStatus = response.IsCorrect.HasValue ? "Graded" : "Pending",
                        GradedAt = null,
                        GradedBy = null
                    });
                }

                _logger.LogInformation($"Retrieved {responseDTOs.Count} responses for question {questionId}");

                return new QuestionResponsesPagedDTO
                {
                    TotalCount = totalCount,
                    Responses = responseDTOs
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question responses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get overall statistics for an exam
        /// </summary>
        public async Task<ExamStatisticsDTO> GetExamStatisticsAsync(int examId)
        {
            try
            {
                // ✅ Get exam details
                var exam = await _context.Exams.FindAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                // ✅ Get all questions
                var questions = await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .ToListAsync();

                var totalMarks = questions.Sum(q => q.Marks);
                var passingMarks = (int)(totalMarks / 2); // Default: 50% of total marks

                // ✅ Get all responses
                var allResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .ToListAsync();

                var uniqueStudents = allResponses
                    .Select(r => r.StudentID)
                    .Distinct()
                    .ToList();

                // ✅ Calculate statistics
                var studentsAttempted = uniqueStudents.Count;
                var totalEnrolledStudents = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Student);

                var studentsNotAttempted = totalEnrolledStudents - studentsAttempted;

                // ✅ Score distribution
                var scoreDistribution = new Dictionary<string, int>
                {
                    { "0-20", 0 },
                    { "21-40", 0 },
                    { "41-60", 0 },
                    { "61-80", 0 },
                    { "81-100", 0 }
                };

                decimal totalScore = 0;
                int highestScore = 0;
                int lowestScore = 100;
                int passedCount = 0;

                foreach (var studentId in uniqueStudents)
                {
                    var studentResponses = allResponses
                        .Where(r => r.StudentID == studentId)
                        .ToList();

                    int studentScore = (int)studentResponses.Sum(r => r.MarksObtained);
                    totalScore += studentScore;

                    if (studentScore > highestScore)
                        highestScore = studentScore;
                    if (studentScore < lowestScore)
                        lowestScore = studentScore;

                    if (studentScore >= passingMarks)
                        passedCount++;

                    // Update score distribution
                    int percentage = totalMarks > 0 ? (int)((studentScore * 100) / totalMarks) : 0;
                    if (percentage <= 20) scoreDistribution["0-20"]++;
                    else if (percentage <= 40) scoreDistribution["21-40"]++;
                    else if (percentage <= 60) scoreDistribution["41-60"]++;
                    else if (percentage <= 80) scoreDistribution["61-80"]++;
                    else scoreDistribution["81-100"]++;
                }

                var averageScore = studentsAttempted > 0 ? totalScore / studentsAttempted : 0;
                var passPercentage = studentsAttempted > 0 ? ((decimal)passedCount / studentsAttempted) * 100 : 0;

                // ✅ Question analysis
                var questionAnalysis = questions.Select(q =>
                {
                    var questionResponses = allResponses
                        .Where(r => r.QuestionID == q.QuestionID)
                        .ToList();

                    var correctCount = questionResponses.Count(r => r.IsCorrect == true);
                    var attemptedCount = questionResponses.Count;
                    var skippedCount = uniqueStudents.Count - attemptedCount;

                    var correctPercentage = attemptedCount > 0 ? ((decimal)correctCount / attemptedCount) * 100 : 0;
                    var difficulty = correctPercentage > 70 ? "Easy" : correctPercentage > 40 ? "Medium" : "Hard";

                    return new QuestionAnalysisDTO
                    {
                        QuestionId = q.QuestionID,
                        QuestionText = q.QuestionText,
                        CorrectPercentage = correctPercentage,
                        Difficulty = difficulty,
                        AttemptedCount = attemptedCount,
                        SkippedCount = skippedCount
                    };
                }).ToList();

                _logger.LogInformation($"Retrieved statistics for exam {examId}");

                return new ExamStatisticsDTO
                {
                    ExamId = examId,
                    ExamName = exam.Title,
                    TotalStudents = totalEnrolledStudents,
                    StudentsAttempted = studentsAttempted,
                    StudentsNotAttempted = studentsNotAttempted,
                    AverageScore = Math.Round(averageScore, 2),
                    HighestScore = highestScore == 0 ? 0 : highestScore,
                    LowestScore = lowestScore == 100 ? 0 : lowestScore,
                    PassPercentage = Math.Round(passPercentage, 2),
                    TotalMarks = (int)totalMarks,
                    PassingMarks = passingMarks,
                    ScoreDistribution = scoreDistribution,
                    QuestionAnalysis = questionAnalysis
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam statistics: {ex.Message}");
                throw;
            }
        }

        private async Task<StudentResponseDTO> MapToStudentResponseDTOAsync(StudentResponse response, Question question)
        {
            // This method is kept for backward compatibility but not actively used
            // Check if there's a grading record for this response
            var gradingRecord = await _context.GradingRecords
                .FirstOrDefaultAsync(gr => gr.ResponseID == response.ResponseID && gr.Status == "Graded");

            return new StudentResponseDTO
            {
                ResponseID = response.ResponseID,
                ExamID = response.ExamID,
                QuestionID = response.QuestionID,
                StudentID = response.StudentID,
                AnswerText = response.AnswerText,
                IsCorrect = response.IsCorrect,
                MarksObtained = response.MarksObtained,
                Feedback = gradingRecord?.Feedback,
                SubmittedAt = response.SubmittedAt,
                QuestionText = question.QuestionText,
                QuestionType = (int)question.QuestionType,
                QuestionMarks = question.Marks
            };
        }

        private StudentResponseDTO MapToStudentResponseDTO(StudentResponse response, Question question)
        {
            return new StudentResponseDTO
            {
                ResponseID = response.ResponseID,
                ExamID = response.ExamID,
                QuestionID = response.QuestionID,
                StudentID = response.StudentID,
                AnswerText = response.AnswerText,
                IsCorrect = response.IsCorrect,
                MarksObtained = response.MarksObtained,
                Feedback = null,  // Feedback will be fetched separately if needed
                SubmittedAt = response.SubmittedAt,
                QuestionText = question.QuestionText,
                QuestionType = (int)question.QuestionType,
                QuestionMarks = question.Marks
            };
        }
    }
}