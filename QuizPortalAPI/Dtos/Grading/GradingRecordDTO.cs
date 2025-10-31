namespace QuizPortalAPI.DTOs.Grading
{
    public class GradingRecordDTO
    {
        public int GradingId { get; set; }
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int GradedByTeacherId { get; set; }
        public string GradedByTeacher { get; set; } = string.Empty;
        public decimal MarksObtained { get; set; }
        public string? Feedback { get; set; }
        public string? Comment { get; set; }
        public DateTime GradedAt { get; set; }
    }
}
