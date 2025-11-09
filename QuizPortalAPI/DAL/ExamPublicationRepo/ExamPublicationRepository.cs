using QuizPortalAPI.Models;
using QuizPortalAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace QuizPortalAPI.DAL.ExamPublicationRepo;

public class ExamPublicationRepository : IExamPublicationRepository
{
    private readonly AppDbContext _context;
    public ExamPublicationRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<ExamPublication?> GetExamPublicationDetailsForAnExamByIdAsync(int examId)
    {
        var publication = await _context.ExamPublications
                            .FirstOrDefaultAsync(ep => ep.ExamID == examId);

        return publication;
    }

    public async Task<ExamPublication?> GetExamPublicationStatusByExamIdAsync(int examId)
    {
        var publication = await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId && ep.Status == "Published");

        return publication;
    }

    public async Task<ExamPublication?> GetExamPublicationDetailsWithPublishedUserByExamId(int examId)
    {
        var publication = await _context.ExamPublications
                    .Include(ep => ep.PublishedByUser)
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId);

        return publication;
    }
    public async Task AddAsync(ExamPublication examPublication)
    {
        _context.ExamPublications.Add(examPublication);
    }
    public async Task UpdateAsync(ExamPublication examPublication)
    {
        _context.ExamPublications.Update(examPublication);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ExamPublication?> GetPublishedExamPublicationAsync(int examId)
    {
        return await _context.ExamPublications
                    .FirstOrDefaultAsync(ep => ep.ExamID == examId && ep.Status == "Published");
    }

    public async Task<bool> IsExamPublishedAsync(int examId)
    {
        return await _context.ExamPublications
                    .AnyAsync(ep => ep.ExamID == examId && ep.Status == "Published");
    }
}