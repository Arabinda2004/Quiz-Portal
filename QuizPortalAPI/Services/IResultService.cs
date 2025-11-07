using QuizPortalAPI.DTOs.Result;

namespace QuizPortalAPI.Services
{
    public interface IResultService
    {
        
        /// <summary>
        /// Get all exam results for a student (paginated)
        /// </summary>
        Task<List<ResultDTO>> GetStudentResultsAsync(int studentId, int page, int pageSize);

        /// <summary>
        /// Get a specific exam result for a student
        /// </summary>
        Task<ResultDTO?> GetStudentExamResultAsync(int examId, int studentId);

        /// <summary>
        /// Get detailed exam result with question-wise breakdown
        /// </summary>
        Task<dynamic> GetExamResultDetailsAsync(int examId, int studentId);


        /// <summary>
        /// Get all results for an exam (paginated)
        /// </summary>
        Task<List<ResultDTO>> GetExamAllResultsAsync(int examId, int teacherId, int page, int pageSize);

        /// <summary>
        /// Get a specific student's result in an exam
        /// </summary>
        // Task<ResultDTO?> GetStudentExamResultForTeacherAsync(int examId, int studentId, int teacherId);

        /// <summary>
        /// Get result statistics for an exam (average, highest, lowest marks)
        /// </summary>
        // Task<dynamic> GetResultStatisticsAsync(int examId, int teacherId);

        /// <summary>
        /// Get result summary (grade distribution, pass/fail counts)
        /// </summary>
        // Task<dynamic> GetResultSummaryAsync(int examId, int teacherId);

        /// <summary>
        /// Get pass/fail breakdown for an exam
        /// </summary>
        // Task<dynamic> GetPassFailBreakdownAsync(int examId, int teacherId);


        /// <summary>
        /// Get result by ID
        /// </summary>
        // Task<ResultDTO?> GetResultByIdAsync(int resultId);

        /// <summary>
        /// Get all results for a student (no pagination, legacy)
        /// </summary>
        // Task<IEnumerable<ResultDTO>> GetStudentResultsLegacyAsync(int studentId);

        /// <summary>
        /// Get all results for an exam with statistics
        /// </summary>
        // Task<ExamResultsDTO> GetExamResultsAsync(int examId, int teacherId);

        /// <summary>
        /// Calculate exam result for a student
        /// </summary>
        // Task<ResultDTO> CalculateExamResultAsync(int examId, int studentId);

        /// <summary>
        /// Grade a subjective response
        /// </summary>
        // Task<bool> GradeSubjectiveResponseAsync(int responseId, int evaluatorId, decimal marksObtained);

        /// <summary>
        /// Publish result for student
        /// </summary>
        // Task<bool> PublishResultAsync(int resultId, int teacherId);

        /// <summary>
        /// Publish all results for an exam (bulk operation)
        /// </summary>
        // Task<dynamic> PublishExamResultsAsync(int examId, int teacherId, decimal passingPercentage = 50);

        /// <summary>
        /// Auto-grade MCQ responses for an exam
        /// </summary>
        // Task<int> AutoGradeMCQResponsesAsync(int examId);

        /// <summary>
        /// Get exam's total marks
        /// </summary>
        Task<decimal> GetExamTotalMarksAsync(int examId);

        /// <summary>
        /// Get student's total marks in an exam
        /// </summary>
        // Task<decimal> GetStudentTotalMarksAsync(int examId, int studentId);

        /// <summary>
        /// Get student's rank in exam
        /// </summary>
        Task<int> GetStudentRankAsync(int examId, int studentId);

        /// <summary>
        /// Recalculate ranks for all students in an exam
        /// </summary>
        Task RecalculateExamRanksAsync(int examId);

        /// <summary>
        /// Check if result exists
        /// </summary>
        // Task<bool> ResultExistsAsync(int examId, int studentId);

        // ============ EXAM PUBLICATION ENDPOINTS ============

        /// <summary>
        /// Check if all responses for an exam are graded
        /// </summary>
        Task<bool> AreAllResponsesGradedAsync(int examId);

        /// <summary>
        /// Get grading progress for an exam
        /// </summary>
        Task<dynamic> GetGradingProgressAsync(int examId, int teacherId);

        /// <summary>
        /// Publish an exam (all results become visible to students)
        /// Only possible if all responses are graded
        /// </summary>
        Task<dynamic> PublishExamAsync(int examId, int teacherId, decimal passingPercentage = 50, string? notes = null);

        /// <summary>
        /// Get exam publication status
        /// </summary>
        Task<dynamic?> GetExamPublicationStatusAsync(int examId, int teacherId);

        /// <summary>
        /// Check if exam is published
        /// </summary>
        Task<bool> IsExamPublishedAsync(int examId);

        /// <summary>
        /// Get all published exams for a student
        /// </summary>
        Task<List<ResultDTO>> GetPublishedExamResultsAsync(int studentId);

        /// <summary>
        /// Unpublish an exam (admin/teacher only, reverses publication)
        /// </summary>
        Task<dynamic> UnpublishExamAsync(int examId, int teacherId, string? reason = null);
    }
}