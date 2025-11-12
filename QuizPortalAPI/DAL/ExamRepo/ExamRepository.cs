using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.ExamRepo;

public class ExamRepository : IExamRepository
{
    private readonly AppDbContext _context;
    public ExamRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Exam?> GetExamWithQuestionsByIdWithCreatorAsync(int examId)
    {
        var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

        return exam;
    }

    public async Task<List<Exam>> GetTeacherExamsWithQuestionsAsync(int teacherId)
    {
        var exams = await _context.Exams
                    .Include(e => e.Questions)
                    .Where(e => e.CreatedBy == teacherId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

        return exams;
    }

    public async Task<List<Exam>> GetAllExamsForAdminAsync()
    {
        var exams = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

        return exams;
    }

    public async Task<Exam?> GetExamDetailsByIdAsync(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);

        return exam ?? null;
    }

    public async Task DeleteAsync(Exam exam)
    {
        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
    }

    public async Task<Exam?> GetExamByAccessCodeAsync(string accessCode)
    {
        var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.Questions)
                    .FirstOrDefaultAsync(e => e.AccessCode == accessCode);

        return exam;
    }

    public async Task<Exam?> GetExamWithQuestionsForStudentAsync(int examId)
    {
        var exam = await _context.Exams
                    .Include(e => e.Questions!)
                        .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

        return exam;
    }

    public async Task CreateAsync(Exam exam)
    {
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
    }

    public async Task<Exam?> GetExamByIdWithCreatorDetails(int examId)
    {
        var exam = await _context.Exams
                    .Include(e => e.CreatedByUser)
                    .FirstOrDefaultAsync(e => e.ExamID == examId);

        return exam;
    }

    public async Task UpdateAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }

    public async Task<Exam?> FindExamByIdAsync(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        return exam;
    }

    public async Task<decimal> GetExamTotalMarksByExamIdAsync(int examId)
    {
        var totalMarks = await _context.Questions
                            .Where(q => q.ExamID == examId)
                            .SumAsync(q => q.Marks);

        return totalMarks;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}