namespace QuizPortalAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication response containing JWT tokens
    /// </summary>
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfoDTO? User { get; set; }
    }

    /// <summary>
    /// User info included in auth response
    /// </summary>
    public class UserInfoDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsDefaultPassword { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
