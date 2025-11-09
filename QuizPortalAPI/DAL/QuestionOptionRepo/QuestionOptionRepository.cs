using QuizPortalAPI.Data;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.QuestionOptionRepo;

public class QuestionOptionRepository : IQuestionOptionRepository
{
    private readonly AppDbContext _context;

    public QuestionOptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task RemoveAsync(QuestionOption option)
    {
        _context.QuestionOptions.Remove(option);
    }

}