using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Result
{
    public class SubmitResultDTO
    {
        [Required(ErrorMessage = "Exam ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be greater than 0")]
        public int ExamID { get; set; }

        [Range(0, 100, ErrorMessage = "Passing percentage must be between 0 and 100")]
        public decimal PassingPercentage { get; set; } = 50; // Default 50% to pass

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
