using QuizPortalAPI.DTOs.Auth;

namespace QuizPortalAPI.Services
{
    /// <summary>
    /// Interface for authentication operations
    /// Handles login, registration, token management, and password changes
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// Returns JWT access token
        /// </summary>
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);

        /// <summary>
        /// Register new user (Student/Teacher - not Admin)
        /// </summary>
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO);

        /// <summary>
        /// Change user password
        /// Requires current password verification
        /// </summary>
        Task<AuthResponseDTO> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<bool> LogoutAsync(int userId);

        /// <summary>
        /// Validate JWT token and return user claims
        /// </summary>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Get user info from valid JWT token
        /// </summary>
        Task<UserInfoDTO?> GetUserFromTokenAsync(string token);
    }
}
