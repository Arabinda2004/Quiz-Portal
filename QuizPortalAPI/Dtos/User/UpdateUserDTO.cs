using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.User
{
    public class UpdateUserDTO
    {
        [MaxLength(40, ErrorMessage = "Full name cannot exceed 40 characters")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        // // Role can be updated by admins
        // public string? Role { get; set; }
    }
}
