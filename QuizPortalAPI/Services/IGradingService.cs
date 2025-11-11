using QuizPortalAPI.DTOs.Grading;

namespace QuizPortalAPI.Services
{
    public interface IGradingService
    {
        // Get pending responses for grading
        Task<PendingResponsesDTO> GetPendingResponsesAsync(int examId, int teacherId);

        // Task<PendingResponsesDTO> GetPendingResponsesByQuestionAsync(int examId, int questionId, int teacherId, int page = 1, int pageSize = 10);

        Task<PendingResponsesDTO> GetPendingResponsesByStudentAsync(int examId, int studentId, int teacherId);

        // Get response details for grading
        Task<ResponseForGradingDTO?> GetResponseForGradingAsync(int responseId, int teacherId);

        // Grade a single response
        Task<bool> GradeSingleResponseAsync(int responseId, int teacherId, GradeSingleResponseDTO gradeDto);

        // Get grading statistics
        Task<GradingStatsDTO> GetGradingStatsAsync(int examId, int teacherId);

        // Regrade a response
        Task<bool> RegradeResponseAsync(int responseId, int teacherId, RegradingDTO regradingDto);
    }
}
