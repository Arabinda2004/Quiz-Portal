using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for user registration request (for students/teachers to self-register)
    /// </summary>
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(40, ErrorMessage = "Full name cannot exceed 40 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Teacher|Student)$", ErrorMessage = "Role must be either 'Teacher' or 'Student'")]
        public string Role { get; set; } = string.Empty;
    }
}
