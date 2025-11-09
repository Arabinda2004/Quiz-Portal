using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.StudentResponse;
using QuizPortalAPI.Models;
using QuizPortalAPI.DAL.StudentResponseRepo;
using QuizPortalAPI.DAL.ExamRepo;
using QuizPortalAPI.DAL.QuestionRepo;
using QuizPortalAPI.DAL.UserRepo;
using QuizPortalAPI.DAL.ExamPublicationRepo;
using QuizPortalAPI.DAL.GradingRecordRepo;
using QuizPortalAPI.DAL.ResultRepo;

namespace QuizPortalAPI.Services
{
    public class StudentResponseService : IStudentResponseService
    {
        private readonly IStudentResponseRepository _studentResponseRepo;
        private readonly IExamRepository _examRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IExamPublicationRepository _examPublicationRepo;
        private readonly IGradingRecordRepository _gradingRecordRepo;
        private readonly IResultRepository _resultRepo;
        private readonly ILogger<StudentResponseService> _logger;

        public StudentResponseService(
            IStudentResponseRepository studentResponseRepo,
            IExamRepository examRepo,
            IQuestionRepository questionRepo,
            IUserRepository userRepo,
            IExamPublicationRepository examPublicationRepo,
            IGradingRecordRepository gradingRecordRepo,
            IResultRepository resultRepo,
            ILogger<StudentResponseService> logger)
        {
            _studentResponseRepo = studentResponseRepo;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _userRepo = userRepo;
            _examPublicationRepo = examPublicationRepo;
            _gradingRecordRepo = gradingRecordRepo;
            _resultRepo = resultRepo;
            _logger = logger;
        }

        /// <summary>
        /// Student submits an answer to a question
        /// </summary>
        public async Task<StudentResponseDTO> SubmitAnswerAsync(int examId, int studentId, CreateStudentResponseDTO createResponseDTO)
        {
            try
            {
                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var now = DateTime.UtcNow;
                if (now < exam.ScheduleStart || now > exam.ScheduleEnd)
                    throw new InvalidOperationException("Exam is not active");

                var publication = await _examPublicationRepo.GetPublishedExamPublicationAsync(examId);

                if (publication != null)
                    throw new InvalidOperationException("This exam has been published and no further submissions are allowed");

                var question = await _questionRepo.FindQuestionByIdAsync(createResponseDTO.QuestionID);
                if (question == null)
                    throw new InvalidOperationException("Question not found");

                if (question.ExamID != examId)
                    throw new InvalidOperationException("Question does not belong to this exam");

                var student = await _userRepo.FindUserByIdAsync(studentId);
                if (student == null)
                    throw new InvalidOperationException("Student not found");


                var existingResponse = await _studentResponseRepo.FindExistingResponseAsync(
                    examId, createResponseDTO.QuestionID, studentId);

                StudentResponse response;

                if (existingResponse != null)
                {
                    response = existingResponse;
                    response.AnswerText = createResponseDTO.AnswerText;
                    response.SubmittedAt = DateTime.UtcNow;

                    await _studentResponseRepo.UpdateAsync(response);
                }
                else
                {
                    response = new StudentResponse
                    {
                        ExamID = examId,
                        QuestionID = createResponseDTO.QuestionID,
                        StudentID = studentId,
                        AnswerText = createResponseDTO.AnswerText,
                        SubmittedAt = DateTime.UtcNow
                    };

                    response.IsCorrect = null;
                    response.MarksObtained = 0;

                    await _studentResponseRepo.AddAsync(response);
                }

                await _studentResponseRepo.SaveChangesAsync();

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
                var response = await _studentResponseRepo.GetResponseByIdWithQuestionAsync(responseId);

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
                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var questions = await _questionRepo.GetExamQuestionsByExamIdAsync(examId);

                var responses = await _studentResponseRepo.GetStudentResponsesWithQuestionsAsync(examId, studentId);

                var responseDTOs = new List<StudentResponseDTO>();
                foreach (var response in responses)
                {
                    var gradingRecord = await _gradingRecordRepo.GetGradedRecordByResponseIdAsync(response.ResponseID);

                    var dto = MapToStudentResponseDTO(response, response.Question!);
                    
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
                var question = await _questionRepo.FindQuestionByIdAsync(questionId);
                if (question == null)
                    throw new InvalidOperationException("Question not found");

                var responses = await _studentResponseRepo.GetResponsesByQuestionWithQuestionAsync(questionId);

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
                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                    return false;

                var now = DateTime.UtcNow;
                bool isExamActive = now >= exam.ScheduleStart && now <= exam.ScheduleEnd;
                
                // Check if exam is published - if published, students cannot submit new answers
                var isExamPublished = await _examPublicationRepo.IsExamPublishedAsync(examId);

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
                return await _studentResponseRepo.CountResponsesByStudentAsync(examId, studentId);
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
                return await _studentResponseRepo.ExistsAsync(examId, questionId, studentId);
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
                var response = await _studentResponseRepo.GetstudentResponseByResponseIdAsync(responseId);
                if (response == null)
                    throw new InvalidOperationException("Response not found");

                // Verify student owns the response
                if (response.StudentID != studentId)
                    throw new UnauthorizedAccessException("You can only withdraw your own responses");

                // Check if exam is still active
                var exam = await _examRepo.FindExamByIdAsync(response.ExamID);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var now = DateTime.UtcNow;
                if (now > exam.ScheduleEnd)
                    throw new InvalidOperationException("Cannot withdraw response after exam ends");

                await _studentResponseRepo.DeleteAsync(response);
                await _studentResponseRepo.SaveChangesAsync();

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
        /// Get all students who attempted an exam with their submission details
        /// </summary>
        public async Task<StudentAttemptsResponseDTO> GetAllStudentAttemptsAsync(int examId, int page, int pageSize)
        {
            try
            {
                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var totalQuestions = await _questionRepo.GetExamQuestionCountByExamIdAsync(examId);

                var responsesGrouped = await _studentResponseRepo.GetResponsesGroupedByStudentAsync(examId);
                
                var studentAttempts = responsesGrouped
                    .Select(g => new { StudentId = g.Key, ResponseCount = g.Value.Count })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalCount = responsesGrouped.Count; // count of total unique students

                var studentAttemptDTOs = new List<StudentAttemptDTO>();

                foreach (var attempt in studentAttempts)
                {
                    var student = await _userRepo.FindUserByIdAsync(attempt.StudentId);
                    if (student == null) continue;

                    var responses = responsesGrouped[attempt.StudentId];

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
        /// Get overall statistics for an exam
        /// </summary>
        public async Task<ExamStatisticsDTO> GetExamStatisticsAsync(int examId)
        {
            try
            {
                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                var questions = await _questionRepo.GetExamQuestionsByExamIdAsync(examId);

                var totalMarks = questions.Sum(q => q.Marks);
                var passingPercentage = exam.PassingPercentage;
                var passingMarks = (int)(totalMarks * passingPercentage / 100);

                var allResponses = await _studentResponseRepo.GetAllResponsesForExamAsync(examId);

                var uniqueStudents = allResponses
                    .Select(r => r.StudentID)
                    .Distinct()
                    .ToList();

                var studentsAttempted = uniqueStudents.Count;
                var totalEnrolledStudents = await _studentResponseRepo.CountTotalStudentsByRoleAsync();

                var studentsNotAttempted = totalEnrolledStudents - studentsAttempted;

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
                var passPercentage = studentsAttempted > 0 ? (decimal)passedCount / studentsAttempted * 100 : 0;

                var questionAnalysis = questions.Select(q =>
                {
                    var questionResponses = allResponses
                        .Where(r => r.QuestionID == q.QuestionID)
                        .ToList();

                    var correctCount = questionResponses.Count(r => r.IsCorrect == true);
                    var attemptedCount = questionResponses.Count;
                    var skippedCount = uniqueStudents.Count - attemptedCount;

                    var correctPercentage = attemptedCount > 0 ? (decimal)correctCount / attemptedCount * 100 : 0;
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
            var gradingRecord = await _gradingRecordRepo.GetGradedRecordByResponseIdAsync(response.ResponseID);

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
                Feedback = null,
                SubmittedAt = response.SubmittedAt,
                QuestionText = question.QuestionText,
                QuestionType = (int)question.QuestionType,
                QuestionMarks = question.Marks
            };
        }

        /// <summary>
        /// Finalize exam submission by creating a Result record
        /// This marks the exam as completed and prevents re-access
        /// </summary>
        public async Task<Result> FinalizeExamSubmissionAsync(int examId, int studentId)
        {
            try
            {
                _logger.LogInformation($"Finalizing exam submission for student {studentId}, exam {examId}");

                var exam = await _examRepo.FindExamByIdAsync(examId);
                if (exam == null)
                {
                    _logger.LogWarning($"Exam {examId} not found when finalizing submission");
                    throw new InvalidOperationException("Exam not found");
                }

                var existingResult = await _resultRepo.FindResultByExamAndStudentAsync(examId, studentId);

                if (existingResult != null)
                {
                    _logger.LogInformation($"Found existing result {existingResult.ResultID} with status {existingResult.Status}");

                    // If result exists but is pending, update it to completed
                    if (existingResult.Status == "Pending")
                    {
                        existingResult.Status = "Completed";
                        existingResult.UpdatedAt = DateTime.UtcNow;
                        await _resultRepo.UpdateAsync(existingResult);
                        await _resultRepo.SaveChangesAsync();
                        _logger.LogInformation($"Updated existing result {existingResult.ResultID} to Completed status");
                        return existingResult;
                    }
                    else
                    {
                        _logger.LogWarning($"Exam already submitted with status {existingResult.Status}");
                        throw new InvalidOperationException("Exam has already been submitted");
                    }
                }

                // Create new result record
                _logger.LogInformation($"Creating new result record for student {studentId}, exam {examId}");

                var result = new Result
                {
                    ExamID = examId,
                    StudentID = studentId,
                    Status = "Completed",
                    TotalMarks = 0, // calculated later by grading
                    Percentage = 0, // calculated later by grading
                    IsPublished = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _resultRepo.AddAsync(result);
                await _resultRepo.SaveChangesAsync();

                _logger.LogInformation($"Successfully created result {result.ResultID} for student {studentId} exam {examId}");
                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error finalizing exam submission for student {studentId}, exam {examId}: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to finalize exam submission: {ex.Message}", ex);
            }
        }
    }
}