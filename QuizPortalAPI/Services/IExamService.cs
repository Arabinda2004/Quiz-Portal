using QuizPortalAPI.DTOs.Exam;

namespace QuizPortalAPI.Services
{
    public interface IExamService
    {
        // Teacher operations
        Task<ExamResponseDTO> CreateExamAsync(int teacherId, CreateExamDTO createExamDTO);
        
        Task<ExamResponseDTO?> GetExamByIdAsync(int examId);
        
        Task<IEnumerable<ExamListDTO>> GetTeacherExamsAsync(int teacherId);
        
        Task<IEnumerable<ExamListDTO>> GetAllExamsAsync(); // Admin only
        
        Task<ExamResponseDTO?> UpdateExamAsync(int examId, int teacherId, UpdateExamDTO updateExamDTO);
        
        Task<bool> DeleteExamAsync(int examId, int teacherId);
        
        // Student operations
        Task<ExamAccessResponseDTO?> ValidateAccessAsync(string accessCode);

        Task<dynamic?> GetExamQuestionsForStudentAsync(int examId);
        
        // Utility methods
        Task<bool> IsTeacherExamOwnerAsync(int examId, int teacherId);
        
        
        string GenerateAccessCode();
    }
}