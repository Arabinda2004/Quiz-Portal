namespace QuizPortalAPI.DTOs.StudentResponse
{
    /// <summary>
    /// DTO for individual question responses (used by teachers for grading)
    /// </summary>
    public class QuestionResponseDTO
    {
        /// <summary>
        /// Response ID
        /// </summary>
        public int ResponseId { get; set; }

        /// <summary>
        /// Student ID
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Student Name
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Student Email
        /// </summary>
        public string StudentEmail { get; set; } = string.Empty;

        /// <summary>
        /// Response/Answer text
        /// </summary>
        public string ResponseText { get; set; } = string.Empty;

        /// <summary>
        /// Selected option text (for MCQ)
        /// </summary>
        public string? SelectedOptionText { get; set; }

        /// <summary>
        /// When the response was submitted
        /// </summary>
        public DateTime SubmissionTime { get; set; }

        /// <summary>
        /// Whether the response has been graded
        /// </summary>
        public bool IsGraded { get; set; }

        /// <summary>
        /// Marks awarded by teacher
        /// </summary>
        public int? MarksAwarded { get; set; }

        /// <summary>
        /// Feedback provided by teacher
        /// </summary>
        public string? Feedback { get; set; }

        /// <summary>
        /// Current grading status
        /// </summary>
        public string GradingStatus { get; set; } = string.Empty; // Pending, Graded

        /// <summary>
        /// When the response was graded
        /// </summary>
        public DateTime? GradedAt { get; set; }

        /// <summary>
        /// Name of teacher who graded
        /// </summary>
        public string? GradedBy { get; set; }
    }
}
