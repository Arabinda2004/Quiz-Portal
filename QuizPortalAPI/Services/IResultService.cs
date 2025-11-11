using QuizPortalAPI.DTOs.Result;

namespace QuizPortalAPI.Services
{
    public interface IResultService
    {
        
        /// <summary>
        /// Get all exam results for a student
        /// </summary>
        Task<List<ResultDTO>> GetStudentResultsAsync(int studentId);

        /// <summary>
        /// Get a specific exam result for a student
        /// </summary>
        Task<ResultDTO?> GetStudentExamResultAsync(int examId, int studentId);

        /// <summary>
        /// Get detailed exam result with question-wise breakdown
        /// </summary>
        Task<dynamic> GetExamResultDetailsAsync(int examId, int studentId);


        /// <summary>
        /// Get all results for an exam
        /// </summary>
        Task<List<ResultDTO>> GetExamAllResultsAsync(int examId, int teacherId);

        
        /// <summary>
        /// Get exam's total marks
        /// </summary>
        Task<decimal> GetExamTotalMarksAsync(int examId);


        /// <summary>
        /// Get student's rank in exam
        /// </summary>
        Task<int> GetStudentRankAsync(int examId, int studentId);

        /// <summary>
        /// Recalculate ranks for all students in an exam
        /// </summary>
        Task RecalculateExamRanksAsync(int examId);

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