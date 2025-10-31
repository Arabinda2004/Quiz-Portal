using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    public enum SubmissionStatus
    {
        InProgress = 0,
        Submitted = 1,
        PartiallyGraded = 2,
        FullyGraded = 3
    }

    public class ExamSubmission
    {
        [Key]
        public int SubmissionID { get; set; }

        [Required]
        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentID { get; set; }
        public User? Student { get; set; }

        [Required]
        public SubmissionStatus Status { get; set; } = SubmissionStatus.InProgress;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; }

        public decimal? ObtainedMarks { get; set; }

        public string? TeacherRemarks { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<StudentResponse>? Responses { get; set; }
    }
}