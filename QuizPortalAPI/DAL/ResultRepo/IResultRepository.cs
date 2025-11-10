using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.ResultRepo;

public interface IResultRepository
{
    Task<Result?> GetDetailedResultByStudentAndExamIdAsync(int studentId, int examId);
    Task<Result?> GetStudentResultByStudentAndExamIdAsync(int studentId, int examId);
    Task<List<Result>> GetStudentsAllResultByIdAsync(int studentId);
    Task<List<Result>> GetAllResultsForAnExamByTeacherIdAsync(int examId);
    Task<List<Result>> GetAllResultsByExamIdAsync(int examId);
    Task UpdateAsync(Result result);
    Task SaveChangesAsync();
    Task<int> CountGradedResultsAsync(int examId);
    Task<Result?> GetExistingResultUsingStudentAndExamIdAsync(int studentId, int examID);
    Task<List<Result>> GetPublishedExamResultsByStudentIdAsync(int studentId);
    Task AddAsync(Result result);
    Task<Result?> GetStudentResultByExamAndStudentIdAsync(int examId, int studentId);
    Task<int> CalculateRankAsync(int examId, decimal totalMarks);
    Task<int> GetDistinctStudentResultCountAsync(int examId);
    Task<Result?> FindResultByExamAndStudentAsync(int examId, int studentId);
    Task<Result?> GetExistingResultOfAStudentByIdAsync(int examId, int studentId);
}