namespace QuizPortalAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for refresh token request
    /// </summary>
    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
