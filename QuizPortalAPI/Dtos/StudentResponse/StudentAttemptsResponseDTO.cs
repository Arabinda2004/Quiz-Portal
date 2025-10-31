namespace QuizPortalAPI.DTOs.StudentResponse
{
    /// <summary>
    /// DTO for student attempts on an exam (for teachers to view)
    /// </summary>
    public class StudentAttemptsResponseDTO
    {
        /// <summary>
        /// Total count of students who attempted
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of students who attempted the exam
        /// </summary>
        public IEnumerable<StudentAttemptDTO> Students { get; set; } = new List<StudentAttemptDTO>();
    }

    /// <summary>
    /// Individual student attempt details
    /// </summary>
    public class StudentAttemptDTO
    {
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
        /// Total questions in exam
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Questions answered by student
        /// </summary>
        public int AnsweredQuestions { get; set; }

        /// <summary>
        /// Questions not answered by student
        /// </summary>
        public int UnansweredQuestions { get; set; }

        /// <summary>
        /// Percentage of questions answered
        /// </summary>
        public decimal ProgressPercentage { get; set; }

        /// <summary>
        /// When student attempted the exam
        /// </summary>
        public DateTime AttemptedAt { get; set; }

        /// <summary>
        /// When student completed/submitted the exam
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Current status of attempt
        /// </summary>
        public string Status { get; set; } = string.Empty; // In-Progress, Completed, Submitted

        /// <summary>
        /// Time spent in the exam (in minutes)
        /// </summary>
        public int TimeSpentMinutes { get; set; }
    }
}
