using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.User
{
    public class AdminCreateUserDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(40, ErrorMessage = "Full name cannot exceed 40 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        // Optional: If not provided, a default password will be generated
        public string? Password { get; set; }
    }
}