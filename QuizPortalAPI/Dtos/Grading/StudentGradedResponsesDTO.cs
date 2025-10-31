namespace QuizPortalAPI.DTOs.Grading
{
    public class StudentGradedResponsesDTO
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ExamID { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public decimal TotalMarksObtained { get; set; }
        public decimal TotalMaxMarks { get; set; }
        public decimal Percentage { get; set; }
        public int TotalQuestions { get; set; }
        public List<GradedResponseItemDTO> GradedResponses { get; set; } = new();
    }

    public class GradedResponseItemDTO
    {
        public int ResponseID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public decimal MaxMarks { get; set; }
        public decimal MarksObtained { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;
        public string? Feedback { get; set; }
        public string? Comment { get; set; }
        public DateTime GradedAt { get; set; }
        public string GradedBy { get; set; } = string.Empty;
    }
}
