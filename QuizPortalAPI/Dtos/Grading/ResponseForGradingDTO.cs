namespace QuizPortalAPI.DTOs.Grading
{
    public class ResponseForGradingDTO
    {
        public int ResponseId { get; set; }
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; // MCQ, Subjective, SAQ, etc.
        public decimal MaxMarks { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;
        public decimal? CurrentMarksObtained { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsGraded { get; set; }
        public DateTime? GradedAt { get; set; }
        public string? GradedByTeacher { get; set; }
        public string? Feedback { get; set; }
    }
}
