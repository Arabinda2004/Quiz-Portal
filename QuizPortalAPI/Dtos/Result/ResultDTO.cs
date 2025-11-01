namespace QuizPortalAPI.DTOs.Result
{
    public class ResultDTO
    {
        public int ResultID { get; set; }

        public int ExamID { get; set; }

        public string ExamName { get; set; } = string.Empty;

        public int StudentID { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public decimal TotalMarks { get; set; }

        public decimal ExamTotalMarks { get; set; }

        public int? Rank { get; set; }

        public decimal Percentage { get; set; }

        public decimal PassingPercentage { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public string? EvaluatorName { get; set; }

        public DateTime? EvaluatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}