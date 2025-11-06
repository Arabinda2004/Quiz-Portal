using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Grading
{
    public class GradeSingleResponseDTO
    {
        [Required(ErrorMessage = "Marks obtained is required")]
        [Range(0, 1000, ErrorMessage = "Marks must be between 0 and maximum marks")]
        public decimal MarksObtained { get; set; }

        [StringLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters")]
        public string? Feedback { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
        public bool IsPartialCredit { get; set; } = false;
    }
}
