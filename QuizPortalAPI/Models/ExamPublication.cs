using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    /// <summary>
    /// Tracks the publication status of exams
    /// An exam can only be published if all student responses are graded
    /// </summary>
    public class ExamPublication
    {
        [Key]
        public int PublicationID { get; set; }

        [Required]
        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        // Publication status: NotPublished, Published, Rejected
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "NotPublished";

        // Number of students who attempted the exam
        [Required]
        public int TotalStudents { get; set; }

        // Number of graded student responses
        [Required]
        public int GradedStudents { get; set; }

        // Passing percentage set by teacher at publication time
        [Range(0, 100)]
        public decimal PassingPercentage { get; set; } = 50;

        // Teacher who published the exam
        [ForeignKey("PublishedByUser")]
        public int? PublishedBy { get; set; }
        public User? PublishedByUser { get; set; }

        // When the exam was published
        public DateTime? PublishedAt { get; set; }

        // Notes from teacher (optional)
        [Column(TypeName = "text")]
        public string? PublicationNotes { get; set; }

        // Track creation
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Track last update
        public DateTime? UpdatedAt { get; set; }
    }
}
