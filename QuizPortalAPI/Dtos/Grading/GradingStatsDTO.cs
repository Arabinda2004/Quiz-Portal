namespace QuizPortalAPI.DTOs.Grading
{
    public class GradingStatsDTO
    {
        public int ExamID { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int TotalStudents { get; set; }
        public int TotalResponses { get; set; }
        public int GradedResponses { get; set; }
        public int PendingResponses { get; set; }
        public decimal GradingPercentage { get; set; }
        public List<QuestionGradingStatsDTO> QuestionStats { get; set; } = new();
    }

    public class QuestionGradingStatsDTO
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public int GradedResponses { get; set; }
        public int PendingResponses { get; set; }
        public decimal AverageMarks { get; set; }
        public decimal MaxMarks { get; set; }
    }
}
