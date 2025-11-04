using System.ComponentModel.DataAnnotations;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DTOs.Question
{
    public class CreateQuestionDTO
    {
        [Required(ErrorMessage = "Question text is required")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Question must be between 10 and 5000 characters")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Question type is required")]
        public QuestionType QuestionType { get; set; }

        [Required(ErrorMessage = "Marks are required")]
        [Range(0.1, 100, ErrorMessage = "Marks must be between 0.1 and 100")]
        public decimal Marks { get; set; }

        [Required(ErrorMessage = "At least one option is required for MCQ")]
        public List<CreateQuestionOptionDTO> Options { get; set; } = new();
    }

    public class CreateQuestionOptionDTO
    {
        [Required(ErrorMessage = "Option text is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Option must be between 1 and 1000 characters")]
        public string OptionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "IsCorrect flag is required")]
        public bool IsCorrect { get; set; }
    }
}