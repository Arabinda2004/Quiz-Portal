using QuizPortalAPI.Models;

namespace QuizPortalAPI.DAL.UserRepo;

public interface IUserRepository
{
    Task<User?> GetUserDetailsByIdAsync(int userId);
    Task<User?> FindUserByEmailAsync(string email);
    Task<bool> IsUserExistsWithEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);

    Task<List<User>> GetAllUsersAsync();
    Task<bool> DeleteAsync(int userId);
    Task<UserRole?> GetUserRoleAsync(int userId);
    Task<User?> FindUserByIdAsync(int userId);
}