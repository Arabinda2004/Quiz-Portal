using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.ExamRepo;


public interface IExamRepository
{
    Task<Exam?> GetExamWithQuestionsByIdWithCreatorAsync(int examId);
    Task<List<Exam>> GetTeacherExamsWithQuestionsAsync(int teacherId);
    Task<List<Exam>> GetAllExamsForAdminAsync();
    Task<Exam?> GetExamDetailsByIdAsync(int examId);
    Task DeleteAsync(Exam exam);
    Task<Exam?> GetExamByAccessCodeAsync(string accessCode);
    Task<Exam?> GetExamWithQuestionsForStudentAsync(int examId);
    Task CreateAsync(Exam exam);
    Task<Exam?> GetExamByIdWithCreatorDetails(int examId);
    Task UpdateAsync(Exam exam);
    Task<Exam?> FindExamByIdAsync(int examId);
    Task<decimal> GetExamTotalMarksByExamIdAsync(int examId);
    Task SaveChangesAsync();
}