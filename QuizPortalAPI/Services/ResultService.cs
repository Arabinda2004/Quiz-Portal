using QuizPortalAPI.DAL.ExamRepo;
using QuizPortalAPI.DAL.ResultRepo;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Result;
using QuizPortalAPI.Models;
using QuizPortalAPI.DAL.StudentResponseRepo;
using QuizPortalAPI.DAL.QuestionRepo;
using QuizPortalAPI.DAL.ExamPublicationRepo;
using QuizPortalAPI.DAL.GradingRecordRepo;

namespace QuizPortalAPI.Services
{
    public class ResultService : IResultService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResultService> _logger;
        private readonly IResultRepository _resultRepository;
        private readonly IExamRepository _examRepository;
        private readonly IStudentResponseRepository _studentResponseRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamPublicationRepository _examPublicationRepository;
        private readonly IGradingRecordRepository _gradingRecordRepository;


        public ResultService(AppDbContext context, ILogger<ResultService> logger, IResultRepository resultRepository, IExamRepository examRepository, IStudentResponseRepository studentResponseRepository, IQuestionRepository questionRepository, IExamPublicationRepository examPublicationRepository, IGradingRecordRepository gradingRecordRepository)
        {
            _context = context;
            _logger = logger;
            _resultRepository = resultRepository;
            _examRepository = examRepository;
            _studentResponseRepository = studentResponseRepository;
            _questionRepository = questionRepository;
            _examPublicationRepository = examPublicationRepository;
            _gradingRecordRepository = gradingRecordRepository;
        }

        /// <summary>
        /// Get student's result for a specific exam
        /// </summary>
        public async Task<ResultDTO?> GetStudentExamResultAsync(int examId, int studentId)
        {
            try
            {
                var result = await _resultRepository.GetDetailedResultByStudentAndExamIdAsync(studentId, examId);

                if (result == null)
                {
                    _logger.LogWarning($"Result not found for exam {examId}, student {studentId}");
                    return null;
                }

                return await MapToResultDTOAsync(result);
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

                var results = await _resultRepository.GetStudentsAllResultByIdAsync(studentId);

                var pagedResult = results.Skip(skip).Take(pageSize).ToList();

                _logger.LogInformation($"Retrieved {pagedResult.Count} results for student {studentId} (page {page})");
                
                var resultDTOs = new List<ResultDTO>();
                foreach (var r in pagedResult)
                {
                    resultDTOs.Add(await MapToResultDTOAsync(r));
                }
                return resultDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving student results: {ex.Message}");
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
                return await _examRepository.GetExamTotalMarksByExamIdAsync(examId);
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
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to view results for exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only view results for your own exams");
                }

                var results = await _resultRepository.GetAllResultsForAnExamByTeacherIdAsync(examId);

                var skip = (page - 1) * pageSize;
                var pagedResult = results.Skip(skip).Take(pageSize).ToList();

                _logger.LogInformation($"Teacher {teacherId} retrieved results for exam {examId} (page {page})");
                
                var resultDTOs = new List<ResultDTO>();
                foreach (var r in pagedResult)
                {
                    resultDTOs.Add(await MapToResultDTOAsync(r));
                }
                return resultDTOs;
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
        /// Get detailed exam result with question-wise breakdown
        /// </summary>
        public async Task<dynamic> GetExamResultDetailsAsync(int examId, int studentId)
        {
            try
            {
                var result = await _resultRepository.GetStudentResultByStudentAndExamIdAsync(studentId, examId);

                if (result == null)
                    throw new InvalidOperationException("Result not found");

                var exam = await _examRepository.FindExamByIdAsync(examId);
                var totalMarks = await GetExamTotalMarksAsync(examId);
                var passingPercentage = exam?.PassingPercentage ?? 40;

                var responses = await _studentResponseRepository.GetStudentResponseForAnExamByIdAsync(examId, studentId);

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
                    PassingPercentage = passingPercentage,
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
        /// Get student's rank in exam
        /// </summary>
        public async Task<int> GetStudentRankAsync(int examId, int studentId)
        {
            try
            {
                // Get the student's total marks from their responses
                var studentMarks = await _studentResponseRepository.GetStudentTotalMarksFromTheirResponseAsync(examId, studentId);

                // Calculate rank based on how many students have higher marks
                // Group by student and calculate their total marks from responses
                var higherScoringStudents = await _studentResponseRepository.CountHigherScoringStudentsAsync(examId, studentId);

                return higherScoringStudents + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating rank: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Recalculate ranks for all students in an exam
        /// </summary>
        public async Task RecalculateExamRanksAsync(int examId)
        {
            try
            {
                // Get all students who have results for this exam
                var results = await _resultRepository.GetAllResultsByExamIdAsync(examId);

                if (results.Count == 0)
                {
                    _logger.LogWarning($"No results found for exam {examId} to recalculate ranks");
                    return;
                }

                // Calculate rank for each student
                foreach (var result in results)
                {
                    result.Rank = await GetStudentRankAsync(examId, result.StudentID);
                    await _resultRepository.UpdateAsync(result);
                }

                await _resultRepository.SaveChangesAsync();
                _logger.LogInformation($"Recalculated ranks for {results.Count} students in exam {examId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recalculating ranks for exam {examId}: {ex.Message}");
                throw;
            }
        }

        private async Task<ResultDTO> MapToResultDTOAsync(Result result)
        {
            // Calculate exam total marks from questions
            var examTotalMarks = await _questionRepository.CalculateExamTotalMarksByExamIdAsync(result.ExamID);

            // Get latest submission date from StudentResponses (synchronous call despite the name)
            var latestSubmission = _studentResponseRepository.GetLatestSubmissionOfAStudentInAnExamAsync(result.ExamID, result.StudentID);

            // Calculate rank if not set
            int rank = result.Rank ?? 0;
            if (rank == 0 || !result.Rank.HasValue)
            {
                var studentMarks = await _studentResponseRepository.GetStudentTotalMarksFromTheirResponseAsync(result.ExamID, result.StudentID);

                // Calculate rank based on how many students have higher marks
                var higherScoringStudents = await _studentResponseRepository.CountStudentsWithHigherMarksAsync(result.ExamID, studentMarks);

                rank = higherScoringStudents + 1;
                _logger.LogInformation($"Calculated rank {rank} for student {result.StudentID} in exam {result.ExamID} (marks: {studentMarks}, higher scoring: {higherScoringStudents})");
            }

            return new ResultDTO
            {
                ResultID = result.ResultID,
                ExamID = result.ExamID,
                ExamName = result.Exam?.Title ?? "Unknown",
                StudentID = result.StudentID,
                StudentName = result.Student?.FullName ?? "Unknown",
                StudentEmail = result.Student?.Email ?? "Unknown",
                TotalMarks = result.TotalMarks,
                ExamTotalMarks = examTotalMarks,
                Rank = rank,
                Percentage = result.Percentage,
                PassingPercentage = result.Exam?.PassingPercentage ?? 40,
                Status = result.Status,
                IsPublished = result.IsPublished,
                EvaluatorName = result.EvaluatorUser?.FullName,
                EvaluatedAt = result.EvaluatedAt,
                PublishedAt = result.PublishedAt,
                SubmittedAt = latestSubmission?.SubmittedAt ?? result.CreatedAt,
                CreatedAt = result.CreatedAt
            };
        }


        /// <summary>
        /// Check if all responses for an exam are graded
        /// </summary>
        public async Task<bool> AreAllResponsesGradedAsync(int examId)
        {
            try
            {
                var totalResponses = await _studentResponseRepository.GetResponseCountOfAnExamByIdAsync(examId);

                if (totalResponses == 0)
                    return false;

                var gradedResponses = await _gradingRecordRepository.GetAllGradedResponseCountByExamIdAsync(examId);

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
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                    throw new UnauthorizedAccessException("You can only view progress for your own exams");

                // Count unique students who have submitted responses
                var totalStudents = await _studentResponseRepository.GetUniqueStudentResponseCountAsync(examId);

                var gradedStudents = await _resultRepository.CountGradedResultsAsync(examId);

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
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to publish exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only publish results for your own exams");
                }

                var existingPublication = await _examPublicationRepository.GetExamPublicationDetailsForAnExamByIdAsync(examId);

                if (existingPublication != null && existingPublication.Status == "Published")
                {
                    throw new InvalidOperationException("This exam has already been published");
                }

                if (passingPercentage < 0 || passingPercentage > 100)
                    throw new ArgumentException("Passing percentage must be between 0 and 100");

                var studentsWithResponses = await _studentResponseRepository.GetAllUniqueStudentsResponseAsync(examId);

                if (studentsWithResponses.Count == 0)
                    throw new InvalidOperationException("No student responses found for this exam");

                foreach (var studentId in studentsWithResponses)
                {
                    var existingResult = await _resultRepository.GetExistingResultUsingStudentAndExamIdAsync(studentId, examId);

                    if (existingResult == null)
                    {
                        
                        var newResult = new Result
                        {
                            ExamID = examId,
                            StudentID = studentId,
                            Status = "Completed",
                            TotalMarks = 0,
                            Percentage = 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Calculate marks from graded responses
                        var studentMarks = await _studentResponseRepository.GetStudentTotalMarksFromTheirResponseAsync(examId, studentId);

                        var examTotalMarks = await _examRepository.GetExamTotalMarksByExamIdAsync(examId);

                        newResult.TotalMarks = studentMarks;
                        newResult.Percentage = examTotalMarks > 0 ? (studentMarks / examTotalMarks) * 100 : 0;

                        await _resultRepository.AddAsync(newResult);
                        _logger.LogInformation($"Created Result record for student {studentId} in exam {examId}");
                    }
                }

                await _resultRepository.SaveChangesAsync();

                // Get all results for the exam (now including newly created ones)
                var results = await _resultRepository.GetAllResultsByExamIdAsync(examId);
                // Check if all responses are graded
                var allGraded = await AreAllResponsesGradedAsync(examId);
                if (!allGraded)
                {
                    var totalResponses = await _studentResponseRepository.GetResponseCountOfAnExamByIdAsync(examId);

                    var gradedResponses = await _gradingRecordRepository.GetAllGradedResponseCountByExamIdAsync(examId);

                    var pendingCount = totalResponses - gradedResponses;

                    throw new InvalidOperationException(
                        $"Cannot publish exam. {pendingCount} out of {totalResponses} responses are still pending grading");
                }

                // Get grading progress
                var totalStudents = results.Count;

                // Recalculate ranks for all students before publishing
                _logger.LogInformation($"Recalculating ranks for {totalStudents} students in exam {examId}");
                foreach (var result in results)
                {
                    // Update total marks first
                    result.TotalMarks = await _studentResponseRepository.GetStudentTotalMarksFromTheirResponseAsync(examId, result.StudentID);
                    
                    // Recalculate rank
                    result.Rank = await GetStudentRankAsync(examId, result.StudentID);
                }

                // Mark all results as published and graded (since all responses are confirmed to be graded)
                var publishedCount = 0;
                foreach (var result in results)
                {
                    result.Status = "Graded"; // Mark as graded when publishing
                    result.IsPublished = true;
                    result.PublishedAt = DateTime.UtcNow;
                    await _resultRepository.UpdateAsync(result);
                    publishedCount++;
                }
                
                var gradedStudents = totalStudents; // All students are now graded

                // Create or update ExamPublication record
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
                    await _examPublicationRepository.AddAsync(existingPublication);
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
                    await _examPublicationRepository.UpdateAsync(existingPublication);
                }

                await _examPublicationRepository.SaveChangesAsync();
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
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                    throw new UnauthorizedAccessException("You can only view publication status for your own exams");

                var publication = await _examPublicationRepository.GetExamPublicationDetailsWithPublishedUserByExamId(examId);

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
                var publication = await _examPublicationRepository.GetExamPublicationStatusByExamIdAsync(examId);

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
                var results = await _resultRepository.GetPublishedExamResultsByStudentIdAsync(studentId);

                _logger.LogInformation($"Retrieved {results.Count} published results for student {studentId}");
                
                var resultDTOs = new List<ResultDTO>();
                foreach (var r in results)
                {
                    resultDTOs.Add(await MapToResultDTOAsync(r));
                }
                return resultDTOs;
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
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to unpublish exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only unpublish exams you own");
                }

                var publication = await _examPublicationRepository.GetExamPublicationDetailsForAnExamByIdAsync(examId);

                if (publication == null || publication.Status != "Published")
                    throw new InvalidOperationException("This exam is not published");

                // Mark all results as unpublished
                var results = await _resultRepository.GetAllResultsByExamIdAsync(examId);

                foreach (var result in results)
                {
                    result.IsPublished = false;
                    await _resultRepository.UpdateAsync(result);
                }

                // Update publication record
                publication.Status = "NotPublished";
                publication.PublishedAt = null;
                publication.PublishedBy = null;
                publication.PublicationNotes = reason;
                publication.UpdatedAt = DateTime.UtcNow;
                await _examPublicationRepository.UpdateAsync(publication);

                await _examPublicationRepository.SaveChangesAsync();
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