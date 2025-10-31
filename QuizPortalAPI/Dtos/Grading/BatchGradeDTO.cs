using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Grading
{
    public class BatchGradeItemDTO
    {
        [Required]
        public int ResponseID { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Marks must be between 0 and maximum marks")]
        public decimal MarksObtained { get; set; }

        public string? Feedback { get; set; }

        public string? Comment { get; set; }
    }

    public class BatchGradeDTO
    {
        [Required]
        public int ExamID { get; set; }

        [Required]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "At least one response must be provided")]
        public IList<BatchGradeItemDTO> Responses { get; set; } = new List<BatchGradeItemDTO>();
    }
}
