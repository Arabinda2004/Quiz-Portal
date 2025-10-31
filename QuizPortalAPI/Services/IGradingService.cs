using QuizPortalAPI.DTOs.Grading;

namespace QuizPortalAPI.Services
{
    public interface IGradingService
    {
        // Get pending responses for grading
        Task<PendingResponsesDTO> GetPendingResponsesAsync(int examId, int teacherId, int page = 1, int pageSize = 10);

        Task<PendingResponsesDTO> GetPendingResponsesByQuestionAsync(int examId, int questionId, int teacherId, int page = 1, int pageSize = 10);

        Task<PendingResponsesDTO> GetPendingResponsesByStudentAsync(int examId, int studentId, int teacherId, int page = 1, int pageSize = 10);

        // Get response details for grading
        Task<ResponseForGradingDTO?> GetResponseForGradingAsync(int responseId, int teacherId);

        // Grade a single response
        Task<bool> GradeSingleResponseAsync(int responseId, int teacherId, GradeSingleResponseDTO gradeDto);

        // Grade multiple responses (batch grading)
        Task<bool> GradeBatchResponsesAsync(int teacherId, BatchGradeDTO batchGradeDto);

        // Get grading history for a question or student
        Task<IEnumerable<GradingRecordDTO>> GetGradingHistoryAsync(int responseId);

        Task<IEnumerable<GradingRecordDTO>> GetQuestionGradingHistoryAsync(int examId, int questionId, int teacherId);

        Task<IEnumerable<GradingRecordDTO>> GetStudentGradingHistoryAsync(int examId, int studentId, int teacherId);

        // Get grading statistics
        Task<GradingStatsDTO> GetGradingStatsAsync(int examId, int teacherId);

        // Mark question as graded
        Task<bool> MarkQuestionGradedAsync(int examId, int questionId, int teacherId);

        // Get graded responses for student
        Task<StudentGradedResponsesDTO> GetStudentGradedResponsesAsync(int examId, int studentId, int teacherId);

        // Regrade a response
        Task<bool> RegradeResponseAsync(int responseId, int teacherId, RegradingDTO regradingDto);
    }
}
