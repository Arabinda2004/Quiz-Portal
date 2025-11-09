using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.ExamPublicationRepo;

public interface IExamPublicationRepository
{
    Task<ExamPublication?> GetExamPublicationDetailsForAnExamByIdAsync(int examId);
    Task<ExamPublication?> GetExamPublicationStatusByExamIdAsync(int examId);
    Task<ExamPublication?> GetExamPublicationDetailsWithPublishedUserByExamId(int examId);
    Task AddAsync(ExamPublication examPublication);
    Task UpdateAsync(ExamPublication examPublication);
    Task SaveChangesAsync();
    Task<ExamPublication?> GetPublishedExamPublicationAsync(int examId);
    Task<bool> IsExamPublishedAsync(int examId);
}