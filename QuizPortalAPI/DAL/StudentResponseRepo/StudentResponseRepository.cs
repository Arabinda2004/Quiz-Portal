using QuizPortalAPI.Models;
using QuizPortalAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace QuizPortalAPI.DAL.StudentResponseRepo;

public class StudentResponseRepository : IStudentResponseRepository
{
    private readonly AppDbContext _context;
    public StudentResponseRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<StudentResponse>> GetStudentResponseForAnExamByIdAsync(int examId, int studentId)
    {
        var responses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Question!.Options)
                    .ToListAsync();

        return responses;
    }

    public async Task<decimal> GetStudentTotalMarksFromTheirResponseAsync(int examId, int studentId)
    {
        var studentMarks = await _context.StudentResponses
            .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
            .SumAsync(sr => (decimal?)sr.MarksObtained) ?? 0;

        return studentMarks;
    }
    public decimal GetStudentTotalMarksFromTheirResponse(int examId, int studentId)
    {
        var studentMarks = _context.StudentResponses
            .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
            .Sum(sr => (decimal?)sr.MarksObtained) ?? 0;

        return studentMarks;
    }

    public async Task<int> CountHigherScoringStudentsAsync(int examId, int studentMarks)
    {
        var count = await _context.StudentResponses
                                    .Where(sr => sr.ExamID == examId)
                                    .GroupBy(sr => sr.StudentID)
                                    .Select(g => new
                                    {
                                        StudentID = g.Key,
                                        TotalMarks = g.Sum(sr => sr.MarksObtained)
                                    })
                                    .Where(s => s.TotalMarks > studentMarks)
                                    .CountAsync();

        return count;
    }

    public StudentResponse? GetLatestSubmissionOfAStudentInAnExamAsync(int examID, int studentId)
    {
        var latestSubmission = _context.StudentResponses
                .Where(sr => sr.ExamID == examID && sr.StudentID == studentId)
                .OrderByDescending(sr => sr.SubmittedAt)
                .FirstOrDefault();

        return latestSubmission;
    }

    public async Task<int> CountStudentsWithHigherMarksAsync(int examId, decimal studentMarks)
    {
        var higherScoringStudents = _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .GroupBy(sr => sr.StudentID)
                    .Select(g => new
                    {
                        StudentID = g.Key,
                        TotalMarks = g.Sum(sr => sr.MarksObtained)
                    })
                    .Count(s => s.TotalMarks > studentMarks);

        return higherScoringStudents;
    }
    public int CountStudentsWithHigherMarks(int examId, decimal studentMarks)
    {
        var higherScoringStudents = _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .GroupBy(sr => sr.StudentID)
                    .Select(g => new
                    {
                        StudentID = g.Key,
                        TotalMarks = g.Sum(sr => sr.MarksObtained)
                    })
                    .Count(s => s.TotalMarks > studentMarks);

        return higherScoringStudents;
    }

    public async Task<int> GetResponseCountOfAnExamByIdAsync(int examId)
    {
        var totalResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .CountAsync();

        return totalResponses;
    }

    public async Task<int> GetUniqueStudentResponseCountAsync(int examId)
    {
        var totalStudents = await _context.StudentResponses
                    .Where(r => r.ExamID == examId)
                    .Select(r => r.StudentID)
                    .Distinct()
                    .CountAsync();

        return totalStudents;
    }

    public async Task<List<int>> GetAllUniqueStudentsResponseAsync(int examId)
    {
        var studentsWithResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .Select(sr => sr.StudentID)
                    .Distinct()
                    .ToListAsync();

        return studentsWithResponses;
    }
    public async Task<List<int>> GetStudentResponsesOfAnExamByStudentIdAsync(int examId, int studentId)
    {
        var studentResponses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Select(sr => sr.ResponseID)
                    .ToListAsync();

        return studentResponses;
    }
    public async Task<StudentResponse?> GetstudentResponseByResponseIdAsync(int responseId)
    {
        return await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Exam)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);
    }
    public async Task UpdateAsync(StudentResponse studentResponse)
    {
        _context.StudentResponses.Update(studentResponse);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<StudentResponse>> GetStudentResponseForAQuestionByIdAsync(int questionId)
    {
        return await _context.StudentResponses
                        .Where(sr => sr.QuestionID == questionId)
                        .ToListAsync();
    }
    public async Task<List<StudentResponse>> GetAllResponsesOfAnExamByIdAsync(int examId)
    {
        return await _context.StudentResponses
                        .Where(sr => sr.ExamID == examId)
                        .ToListAsync();
    }
    public async Task<StudentResponse?> GetStudentResponseIncludingQuestionExamAndStudentByResponseIdAsync(int responseId)
    {
        return await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Exam)
                    .Include(sr => sr.Student)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);
    }
    public async Task<List<StudentResponse>> GetPendingResponsesAsync(int examId, int studentId)
    {
        return await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Student)
                    .Where(sr => sr.ExamID == examId &&
                                 sr.StudentID == studentId &&
                                 !_context.GradingRecords
                                     .Any(gr => gr.ResponseID == sr.ResponseID && gr.Status == "Graded"))
                    .OrderByDescending(r => r.SubmittedAt)
                    .ToListAsync();
    }
    public async Task<List<StudentResponse>> GetAllStudentResponsesAsync(int examId, int studentId)
    {
        return await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .ToListAsync();
    }
    public async Task<List<StudentResponse>> GetAllSubmittedResponsesByExamIdAsync(int examId)
    {
        return await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .Include(sr => sr.Student)
                    .Where(sr => sr.ExamID == examId)
                    .OrderByDescending(r => r.SubmittedAt) // latest submission first
                    .ToListAsync();
    }

    public async Task<StudentResponse?> GetResponseByIdWithQuestionAsync(int responseId)
    {
        return await _context.StudentResponses
                    .Include(sr => sr.Question)
                    .FirstOrDefaultAsync(sr => sr.ResponseID == responseId);
    }

    public async Task<StudentResponse?> FindExistingResponseAsync(int examId, int questionId, int studentId)
    {
        return await _context.StudentResponses
                    .FirstOrDefaultAsync(sr => sr.ExamID == examId &&
                                               sr.QuestionID == questionId &&
                                               sr.StudentID == studentId);
    }

    public async Task AddAsync(StudentResponse response)
    {
        await _context.StudentResponses.AddAsync(response);
    }

    public async Task<List<StudentResponse>> GetStudentResponsesWithQuestionsAsync(int examId, int studentId)
    {
        return await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId && sr.StudentID == studentId)
                    .Include(sr => sr.Question)
                    .ToListAsync();
    }

    public async Task<List<StudentResponse>> GetResponsesByQuestionWithQuestionAsync(int questionId)
    {
        return await _context.StudentResponses
                    .Where(sr => sr.QuestionID == questionId)
                    .Include(sr => sr.Question)
                    .ToListAsync();
    }

    public async Task DeleteAsync(StudentResponse response)
    {
        _context.StudentResponses.Remove(response);
    }

    public async Task<bool> ExistsAsync(int examId, int questionId, int studentId)
    {
        return await _context.StudentResponses
                    .AnyAsync(sr => sr.ExamID == examId && 
                                   sr.QuestionID == questionId && 
                                   sr.StudentID == studentId);
    }

    public async Task<int> CountResponsesByStudentAsync(int examId, int studentId)
    {
        return await _context.StudentResponses
                    .CountAsync(sr => sr.ExamID == examId && sr.StudentID == studentId);
    }

    public async Task<List<StudentResponse>> GetAllResponsesForExamAsync(int examId)
    {
        return await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .ToListAsync();
    }

    public async Task<List<int>> GetUniqueStudentIdsForExamAsync(int examId)
    {
        return await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .Select(sr => sr.StudentID)
                    .Distinct()
                    .ToListAsync();
    }

    public async Task<int> CountTotalStudentsByRoleAsync()
    {
        return await _context.Users
                    .CountAsync(u => u.Role == UserRole.Student);
    }

    public async Task<Dictionary<int, List<StudentResponse>>> GetResponsesGroupedByStudentAsync(int examId)
    {
        var responses = await _context.StudentResponses
                    .Where(sr => sr.ExamID == examId)
                    .ToListAsync();

        return responses.GroupBy(r => r.StudentID)
                       .ToDictionary(g => g.Key, g => g.ToList());
    }
}
