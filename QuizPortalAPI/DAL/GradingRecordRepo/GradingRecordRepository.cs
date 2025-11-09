using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.GradingRecordRepo;

public class GradingRecordRepository : IGradingRecordRepository
{
    private readonly AppDbContext _context;

    public GradingRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetAllGradedResponseCountByExamIdAsync(int examId)
    {
        var gradedResponses = await _context.GradingRecords
                    .Include(gr => gr.StudentResponse)
                    .Where(gr => gr.StudentResponse != null && gr.StudentResponse.ExamID == examId)
                    .Select(gr => gr.ResponseID)
                    .Distinct()
                    .CountAsync();
        return gradedResponses;
    }

    public async Task<List<int>> CheckHowManyResponsesAreGradedAsync(List<int> studentResponses)
    {
        return await _context.GradingRecords
                    .Where(gr => studentResponses.Contains(gr.ResponseID) && gr.Status == "Graded")
                    .Select(gr => gr.ResponseID)
                    .Distinct()
                    .ToListAsync();
    }
    public async Task<GradingRecord?> GetGradingRecordUsingResponseIdAsync(int responseId)
    {
        return await _context.GradingRecords
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");
    }
    public async Task UpdateAsync(GradingRecord gradingRecord)
    {
        _context.GradingRecords.Update(gradingRecord);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task AddAsync(GradingRecord gradingRecord)
    {
        _context.GradingRecords.Add(gradingRecord);
    }
    public async Task<int> GetGradingRecordsBasedOnQuestionIdAsync(int questionId)
    {
        return await _context.GradingRecords
                        .CountAsync(gr => gr.QuestionID == questionId && gr.Status == "Graded");
    }
    public async Task<double> GetAverageMarksByQuestionIdAsync(int questionId)
    {
        return await _context.GradingRecords
                            .Where(gr => gr.QuestionID == questionId && gr.Status == "Graded")
                            .AverageAsync(gr => (double)gr.MarksObtained);
    }
    public async Task<int> GetGradedQuestionsForStudentAsync(int studentId, int examId)
    {
        return await _context.GradingRecords
                            .Where(gr => gr.StudentID == studentId && gr.QuestionID != 0)
                            .Join(
                                _context.StudentResponses.Where(sr => sr.ExamID == examId),
                                gr => gr.ResponseID,
                                sr => sr.ResponseID,
                                (gr, sr) => gr
                            )
                            .Where(gr => gr.Status == "Graded")
                            .CountAsync();
    }
    public async Task RemoveAsync(GradingRecord gradingRecord)
    {
        _context.GradingRecords.Remove(gradingRecord);
    }
    public async Task<GradingRecord?> GetGradingRecordWithEvaluatorByResponseIdAsync(int responseId)
    {
        return await _context.GradingRecords
                    .Include(gr => gr.GradedByTeacher)
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");
    }
    public async Task<Dictionary<int, GradingRecord>> GetAllGradingRecordsFromSubmittedResponsesAsync(List<StudentResponse> allResponses)
    {
        return await _context.GradingRecords
                    .Where(gr => allResponses.Select(sr => sr.ResponseID).Contains(gr.ResponseID) && gr.Status == "Graded")
                    .ToDictionaryAsync(gr => gr.ResponseID, gr => gr); 
    }

    public async Task<GradingRecord?> GetGradedRecordByResponseIdAsync(int responseId)
    {
        return await _context.GradingRecords
                    .FirstOrDefaultAsync(gr => gr.ResponseID == responseId && gr.Status == "Graded");
    }
}
