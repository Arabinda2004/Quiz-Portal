using QuizPortalAPI.DTOs.Question;

namespace QuizPortalAPI.Services
{
    public interface IQuestionService
    {
        // Question CRUD operations
        Task<QuestionResponseDTO> CreateQuestionAsync(int examId, int teacherId, CreateQuestionDTO createQuestionDTO);

        Task<QuestionResponseDTO?> GetQuestionByIdAsync(int questionId);

        Task<IEnumerable<QuestionListDTO>> GetExamQuestionsAsync(int examId);

        Task<QuestionResponseDTO?> UpdateQuestionAsync(int questionId, int teacherId, UpdateQuestionDTO updateQuestionDTO);

        Task<bool> DeleteQuestionAsync(int questionId, int teacherId);

        // Utility methods
        Task<bool> IsTeacherQuestionOwnerAsync(int questionId, int teacherId);

        Task<bool> IsExamOwnerAsync(int examId, int teacherId);

        Task<int> GetExamQuestionCountAsync(int examId);

        Task<decimal> GetExamTotalMarksAsync(int examId);
    }
}