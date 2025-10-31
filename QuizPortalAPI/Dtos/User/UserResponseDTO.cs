namespace QuizPortalAPI.DTOs.User
{
    public class UserResponseDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsDefaultPassword { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
