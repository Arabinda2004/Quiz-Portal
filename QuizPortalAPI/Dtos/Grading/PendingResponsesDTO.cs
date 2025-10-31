namespace QuizPortalAPI.DTOs.Grading
{
    public class PendingResponsesDTO
    {
        public int ExamID { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int TotalPending { get; set; }
        public int TotalResponses { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<PendingResponseItemDTO> Responses { get; set; } = new();
    }

    public class PendingResponseItemDTO
    {
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentAnswer { get; set; } = string.Empty;
        public decimal MaxMarks { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public int PendingQuestionsFromStudent { get; set; }
    }
}
