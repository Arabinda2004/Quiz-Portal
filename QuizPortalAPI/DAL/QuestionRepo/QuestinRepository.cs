using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.QuestionRepo;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;
    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
    }

    public async Task<Question?> GetQuestionsWithOptionsbyIdAsync(int questionId)
    {
        var question = await _context.Questions
                        .Include(q => q.Options)
                        .FirstOrDefaultAsync(q => q.QuestionID == questionId);

        return question;
    }

    public async Task<List<Question>> GetQuestionsWithOptionsForAnExamByIdAsync(int examId)
    {
        var questions = await _context.Questions
                        .Where(q => q.ExamID == examId)
                        .Include(q => q.Options)
                        .OrderBy(q => q.QuestionID)
                        .ToListAsync();
        return questions;
    }

    public async Task UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task<Question?> FindQuestionWithIdAsync(int questionId)
    {
        var question = await _context.Questions.FindAsync(questionId);
        return question;
    }

    public async Task DeleteAsync(Question question)
    {
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetExamQuestionCountByExamIdAsync(int examId)
    {
        return await _context.Questions.CountAsync(q => q.ExamID == examId);
    }

    public decimal CalculateExamTotalMarksByExamId(int examId)
    {
        var totalMarks = _context.Questions
                .Where(q => q.ExamID == examId)
                .Sum(q => (decimal?)q.Marks) ?? 0;

        return totalMarks;
    }
    public async Task<decimal> CalculateExamTotalMarksByExamIdAsync(int examId)
    {
        var totalMarks = await _context.Questions
                .Where(q => q.ExamID == examId)
                .SumAsync(q => (decimal?)q.Marks) ?? 0;

        return totalMarks;
    }
    public async Task<List<Question>> GetExamQuestionsByExamIdAsync(int examId)
    {
        return await _context.Questions
                    .Where(q => q.ExamID == examId)
                    .ToListAsync();
    }

    public async Task<Question?> FindQuestionByIdAsync(int questionId)
    {
        return await _context.Questions.FindAsync(questionId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}