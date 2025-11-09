using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.QuestionOptionRepo;

public interface IQuestionOptionRepository
{
    Task RemoveAsync(QuestionOption option);
    
}