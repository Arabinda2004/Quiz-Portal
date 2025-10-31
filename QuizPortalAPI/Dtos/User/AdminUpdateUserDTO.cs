using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.User
{
    public class AdminUpdateUserDTO
    {
        [MaxLength(40, ErrorMessage = "Full name cannot exceed 40 characters")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        // Only admins can update role
        public string? Role { get; set; }
    }
}