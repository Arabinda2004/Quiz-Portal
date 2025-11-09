using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.StudentResponseRepo;

public interface IStudentResponseRepository
{
    Task<List<StudentResponse>> GetStudentResponseForAnExamByIdAsync(int examId, int studentId);
    Task<decimal> GetStudentTotalMarksFromTheirResponseAsync(int examId, int studentId);
    decimal GetStudentTotalMarksFromTheirResponse(int examId, int studentId);
    Task<int> CountHigherScoringStudentsAsync(int examId, int studentMarks);
    StudentResponse? GetLatestSubmissionOfAStudentInAnExamAsync(int examID, int studentId);
    Task<int> CountStudentsWithHigherMarksAsync(int examId, decimal studentMarks);
    int CountStudentsWithHigherMarks(int examId, decimal studentMarks);
    Task<int> GetResponseCountOfAnExamByIdAsync(int examId);
    Task<int> GetUniqueStudentResponseCountAsync(int examId);
    Task<List<int>> GetAllUniqueStudentsResponseAsync(int examId);
    Task<List<int>> GetStudentResponsesOfAnExamByStudentIdAsync(int examId, int studentId);
    Task<StudentResponse?> GetstudentResponseByResponseIdAsync(int responseId);
    Task UpdateAsync(StudentResponse studentResponse);
    Task SaveChangesAsync();
    Task<List<StudentResponse>> GetStudentResponseForAQuestionByIdAsync(int questionId);
    Task<List<StudentResponse>> GetAllResponsesOfAnExamByIdAsync(int examId);
    Task<StudentResponse?> GetStudentResponseIncludingQuestionExamAndStudentByResponseIdAsync(int responseId);
    Task<List<StudentResponse>> GetPendingResponsesAsync(int examId, int studentId);
    Task<List<StudentResponse>> GetAllStudentResponsesAsync(int examId, int studentId);
    Task<List<StudentResponse>> GetAllSubmittedResponsesByExamIdAsync(int examId);
    Task<StudentResponse?> GetResponseByIdWithQuestionAsync(int responseId);
    Task<StudentResponse?> FindExistingResponseAsync(int examId, int questionId, int studentId);
    Task AddAsync(StudentResponse response);
    Task<List<StudentResponse>> GetStudentResponsesWithQuestionsAsync(int examId, int studentId);
    Task<List<StudentResponse>> GetResponsesByQuestionWithQuestionAsync(int questionId);
    Task DeleteAsync(StudentResponse response);
    Task<bool> ExistsAsync(int examId, int questionId, int studentId);
    Task<int> CountResponsesByStudentAsync(int examId, int studentId);
    Task<List<StudentResponse>> GetAllResponsesForExamAsync(int examId);
    Task<List<int>> GetUniqueStudentIdsForExamAsync(int examId);
    Task<int> CountTotalStudentsByRoleAsync();
    Task<Dictionary<int, List<StudentResponse>>> GetResponsesGroupedByStudentAsync(int examId);
}