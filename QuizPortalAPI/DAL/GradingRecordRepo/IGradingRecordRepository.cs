using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.GradingRecordRepo;

public interface IGradingRecordRepository
{
    Task<int> GetAllGradedResponseCountByExamIdAsync(int examId);
    Task<List<int>> CheckHowManyResponsesAreGradedAsync(List<int> studentResponses);
    Task<GradingRecord?> GetGradingRecordUsingResponseIdAsync(int responseId);
    Task UpdateAsync(GradingRecord gradingRecord);
    Task SaveChangesAsync();
    Task AddAsync(GradingRecord gradingRecord);
    Task<int> GetGradingRecordsBasedOnQuestionIdAsync(int questionId);
    Task<double> GetAverageMarksByQuestionIdAsync(int questionId);
    Task<int> GetGradedQuestionsForStudentAsync(int studentId, int examId);
    Task RemoveAsync(GradingRecord gradingRecord);
    Task<GradingRecord?> GetGradingRecordWithEvaluatorByResponseIdAsync(int responseId);
    Task<Dictionary<int, GradingRecord>> GetAllGradingRecordsFromSubmittedResponsesAsync(List<StudentResponse> allResponses);
    Task<GradingRecord?> GetGradedRecordByResponseIdAsync(int responseId);
}
