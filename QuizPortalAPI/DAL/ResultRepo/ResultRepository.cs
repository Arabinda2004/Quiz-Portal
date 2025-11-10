using QuizPortalAPI.Models;
using QuizPortalAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizPortalAPI.DAL.ResultRepo;

public class ResultRepository : IResultRepository
{
    private readonly AppDbContext _context;

    public ResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result?> GetDetailedResultByStudentAndExamIdAsync(int studentId, int examId)
    {
        var result = await _context.Results
                        .Include(r => r.Exam)
                            .ThenInclude(e => e!.Questions)
                        .Include(r => r.Student)
                        .Include(r => r.EvaluatorUser)
                        .FirstOrDefaultAsync(r => r.StudentID == studentId && r.ExamID == examId);

        return result;
    }

    public async Task<List<Result>> GetStudentsAllResultByIdAsync(int studentId)
    {

        var results = await _context.Results
            .Where(r => r.StudentID == studentId)
            .Include(r => r.Exam)
                .ThenInclude(e => e!.Questions)
            .Include(r => r.Student)
            .Include(r => r.EvaluatorUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return results;
    }

    public async Task<List<Result>> GetAllResultsForAnExamByTeacherIdAsync(int examId)
    {
        var results = await _context.Results
            .Where(r => r.ExamID == examId)
            .Include(r => r.Exam)
            .Include(r => r.Student)
            .Include(r => r.EvaluatorUser)
            .OrderByDescending(r => r.TotalMarks)
            .ToListAsync();

        return results;
    }

    public async Task<Result?> GetStudentResultByStudentAndExamIdAsync(int studentId, int examId)
    {
        var result = await _context.Results
                    .Include(r => r.Exam)
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

        return result;
    }

    public async Task<List<Result>> GetAllResultsByExamIdAsync(int examId)
    {
        var results = await _context.Results
                    .Where(r => r.ExamID == examId)
                    .ToListAsync();

        return results;
    }

    public async Task UpdateAsync(Result result)
    {
        _context.Results.Update(result);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountGradedResultsAsync(int examId)
    {
        var gradedStudents = await _context.Results
                    .Where(r => r.ExamID == examId && r.Status == "Graded")
                    .CountAsync();

        return gradedStudents;
    }

    public async Task<Result?> GetExistingResultUsingStudentAndExamIdAsync(int studentId, int examId)
    {
        var existingResult = await _context.Results
                        .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);

        return existingResult;
    }

    public async Task<List<Result>> GetPublishedExamResultsByStudentIdAsync(int studentId)
    {
        var results = await _context.Results
                    .Where(r => r.StudentID == studentId && r.IsPublished)
                    .Include(r => r.Exam)
                    .Include(r => r.Student)
                    // .Include(r => r.TotalMarks)
                    .Include(r => r.EvaluatorUser)
                    .OrderByDescending(r => r.PublishedAt)
                    .ToListAsync();

        return results;
    }
    public async Task AddAsync(Result result)
    {
        _context.Results.Add(result);
    }
    public async Task<Result?> GetStudentResultByExamAndStudentIdAsync(int examId, int studentId)
    {
        return await _context.Results
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);
    }

    public async Task<int> CalculateRankAsync(int examId, decimal totalMarks)
    {
        return await _context.Results
                    .Where(r => r.ExamID == examId && r.TotalMarks > totalMarks)
                    .CountAsync();
    }
    public async Task<int> GetDistinctStudentResultCountAsync(int examId)
    {
        return await _context.Results
                        .Where(r => r.ExamID == examId)
                        .Select(r => r.StudentID)
                        .Distinct()
                        .CountAsync();
    }

    public async Task<Result?> FindResultByExamAndStudentAsync(int examId, int studentId)
    {
        return await _context.Results
                    .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId);
    }
    public async Task<Result?> GetExistingResultOfAStudentByIdAsync(int examId, int studentId)
    {
        return await _context.Results
                        .FirstOrDefaultAsync(r => r.ExamID == examId && r.StudentID == studentId &&
                            (r.Status == "Completed" || r.Status == "Graded"));
    }
}