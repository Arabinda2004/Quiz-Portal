using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.QuestionRepo;

public interface IQuestionRepository
{
    Task AddAsync(Question question);
    Task<Question?> GetQuestionsWithOptionsbyIdAsync(int questionId);
    Task<List<Question>> GetQuestionsWithOptionsForAnExamByIdAsync(int examId);
    Task UpdateAsync(Question question);
    Task<Question?> FindQuestionWithIdAsync(int questionId);
    Task DeleteAsync(Question question);
    Task<int> GetExamQuestionCountByExamIdAsync(int examId);
    decimal CalculateExamTotalMarksByExamId(int examId);
    Task<decimal> CalculateExamTotalMarksByExamIdAsync(int examId);
    Task<List<Question>> GetExamQuestionsByExamIdAsync(int examId);
    Task<Question?> FindQuestionByIdAsync(int questionId);
    Task SaveChangesAsync();
}