using System.ComponentModel.DataAnnotations;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.DTOs.Question
{
    public class UpdateQuestionDTO
    {
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Question must be between 10 and 5000 characters")]
        public string? QuestionText { get; set; }

        [Range(0.1, 100, ErrorMessage = "Marks must be between 0.1 and 100")]
        public decimal? Marks { get; set; }

        public List<UpdateQuestionOptionDTO>? Options { get; set; }
    }

    public class UpdateQuestionOptionDTO
    {
        public int? OptionID { get; set; }  // If null, it's a new option

        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Option must be between 1 and 1000 characters")]
        public string? OptionText { get; set; }

        public bool? IsCorrect { get; set; }
    }
}