using QuizPortalAPI.Models;

namespace QuizPortalAPI.DTOs.Question
{
    public class QuestionListDTO
    {
        public int QuestionID { get; set; }

        public int ExamID { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public QuestionType QuestionType { get; set; }

        public decimal Marks { get; set; }

        public decimal NegativeMarks { get; set; }

        public int OptionCount { get; set; }  // Number of options (for MCQ)

        public DateTime CreatedAt { get; set; }
    }
}