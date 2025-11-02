using QuizPortalAPI.DTOs.StudentResponse;

namespace QuizPortalAPI.Services
{
    public interface IStudentResponseService
    {
        // Student submission operations
        Task<StudentResponseDTO> SubmitAnswerAsync(int examId, int studentId, CreateStudentResponseDTO createResponseDTO);

        Task<StudentResponseDTO?> GetResponseByIdAsync(int responseId);

        Task<StudentExamResponsesDTO> GetStudentExamResponsesAsync(int examId, int studentId);

        Task<IEnumerable<StudentResponseDTO>> GetExamResponsesByQuestionAsync(int questionId);

        Task<bool> WithdrawResponseAsync(int responseId, int studentId);

        // Teacher/Admin operations for viewing responses
        Task<StudentAttemptsResponseDTO> GetAllStudentAttemptsAsync(int examId, int page, int pageSize);

        Task<QuestionResponsesPagedDTO> GetExamResponsesByQuestionPagedAsync(int questionId, int examId, int page, int pageSize);

        Task<QuestionStatisticsDTO> GetQuestionStatisticsAsync(int questionId, int examId);

        Task<ExamStatisticsDTO> GetExamStatisticsAsync(int examId);

        // Utility methods
        Task<bool> CanSubmitAnswerAsync(int examId, int studentId);

        Task<int> GetStudentResponseCountAsync(int examId, int studentId);

        Task<bool> ResponseExistsAsync(int examId, int questionId, int studentId);

        // Exam submission finalization
        Task<QuizPortalAPI.Models.Result> FinalizeExamSubmissionAsync(int examId, int studentId);
    }
}