namespace QuizPortalAPI.DTOs.Result
{
    public class ExamPublicationDTO
    {
        public int PublicationID { get; set; }

        public int ExamID { get; set; }

        public string ExamTitle { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int TotalStudents { get; set; }

        public int GradedStudents { get; set; }

        public int PendingStudents => TotalStudents - GradedStudents;

        public decimal PassingPercentage { get; set; }

        public int? PublishedBy { get; set; }

        public string? PublishedByName { get; set; }

        public DateTime? PublishedAt { get; set; }

        public string? PublicationNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for publishing an exam
    /// </summary>
    public class PublishExamRequestDTO
    {
        /// <summary>
        /// Passing percentage for the exam (0-100)
        /// </summary>
        public decimal PassingPercentage { get; set; } = 50;

        /// <summary>
        /// Optional notes from teacher about the publication
        /// </summary>
        public string? PublicationNotes { get; set; }
    }

    /// <summary>
    /// DTO for exam publication response
    /// </summary>
    public class PublishExamResponseDTO
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public ExamPublicationDTO? PublicationDetails { get; set; }

        public int? TotalStudentsPending { get; set; }

        public List<string>? ValidationErrors { get; set; }
    }

    /// <summary>
    /// DTO for checking exam publication status
    /// </summary>
    public class ExamPublicationStatusDTO
    {
        public int ExamID { get; set; }

        public string ExamTitle { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public int TotalStudents { get; set; }

        public int GradedStudents { get; set; }

        public int PendingStudents => TotalStudents - GradedStudents;

        public decimal GradingProgress => TotalStudents > 0 ? (GradedStudents * 100m) / TotalStudents : 0;

        public string? PublishedAt { get; set; }

        public string? PublishedBy { get; set; }

        public string? PublicationNotes { get; set; }
    }

    /// <summary>
    /// DTO for unpublishing an exam
    /// </summary>
    public class UnpublishExamRequestDTO
    {
        /// <summary>
        /// Optional reason for unpublishing
        /// </summary>
        public string? Reason { get; set; }
    }
}
