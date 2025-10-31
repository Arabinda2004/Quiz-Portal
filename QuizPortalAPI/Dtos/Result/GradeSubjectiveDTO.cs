using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Result
{
    public class GradeSubjectiveDTO
    {
        [Required(ErrorMessage = "Response ID is required")]
        public int ResponseID { get; set; }

        [Required(ErrorMessage = "Marks obtained is required")]
        [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100")]
        public decimal MarksObtained { get; set; }

        [StringLength(500, ErrorMessage = "Feedback cannot exceed 500 characters")]
        public string? Feedback { get; set; }
    }
}