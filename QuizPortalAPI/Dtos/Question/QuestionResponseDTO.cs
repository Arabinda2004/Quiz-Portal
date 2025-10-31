using QuizPortalAPI.Models;

namespace QuizPortalAPI.DTOs.Question
{
    public class QuestionResponseDTO
    {
        public int QuestionID { get; set; }

        public int ExamID { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; }

        public decimal Marks { get; set; }

        public decimal NegativeMarks { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<QuestionOptionResponseDTO> Options { get; set; } = new();
    }

    public class QuestionOptionResponseDTO
    {
        public int OptionID { get; set; }

        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}