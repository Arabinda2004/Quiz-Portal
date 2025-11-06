using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Exam
{
    public class AccessExamDTO
    {
        [Required(ErrorMessage = "Access code is required")]
        [StringLength(50, ErrorMessage = "Invalid access code")]
        public string AccessCode { get; set; } = string.Empty;

        // [Required(ErrorMessage = "Access password is required")]
        // [StringLength(100, ErrorMessage = "Invalid password")]
        // public string AccessPassword { get; set; } = string.Empty;
    }
}