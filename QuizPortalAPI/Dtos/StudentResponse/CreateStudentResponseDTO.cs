using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.StudentResponse
{
    public class CreateStudentResponseDTO
    {
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Answer text is required")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Answer must be between 1 and 5000 characters")]
        public string AnswerText { get; set; } = string.Empty;
    }
}