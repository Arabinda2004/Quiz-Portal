using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Grading
{
    public class RegradingDTO
    {
        [Required(ErrorMessage = "Reason for regrading is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "New marks obtained is required")]
        [Range(0, 1000, ErrorMessage = "Marks must be between 0 and maximum marks")]
        public decimal NewMarksObtained { get; set; }

        [StringLength(1000, ErrorMessage = "New feedback cannot exceed 1000 characters")]
        public string? NewFeedback { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}
