namespace QuizPortalAPI.DTOs.StudentResponse
{
    public class StudentResponseDTO
    {
        public int ResponseID { get; set; }

        public int ExamID { get; set; }

        public int QuestionID { get; set; }

        public int StudentID { get; set; }

        public string AnswerText { get; set; } = string.Empty;

        public bool? IsCorrect { get; set; }

        public decimal MarksObtained { get; set; }

        public DateTime SubmittedAt { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public int QuestionType { get; set; }

        public decimal QuestionMarks { get; set; }

        public string? Feedback { get; set; }

        /// <summary>
        /// Indicates whether this response has been graded by a teacher
        /// </summary>
        public bool IsGraded { get; set; } = false;
    }
}