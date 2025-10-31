using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(int userId);
        Task<UserResponseDTO?> GetUserByEmailAsync(string email);
        Task<UserResponseDTO> CreateUserAsync(AdminCreateUserDTO createUserDTO);
        Task<UserResponseDTO?> AdminUpdateUserAsync(int userId, AdminUpdateUserDTO updateUserDTO);
        Task<UserResponseDTO?> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO);
        Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDTO changePasswordDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> CanUpdateRoleAsync(int adminUserId, int targetUserId);
        Task<UserRole?> GetUserRoleAsync(int userId);
        Task MarkPasswordAsDefaultAsync(int userId);
        Task<bool> VerifyPasswordAsync(int userId, string password);
    }
}
