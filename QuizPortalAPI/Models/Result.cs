using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentID { get; set; }
        public User? Student { get; set; }

        [Range(0, 10000)]
        public decimal TotalMarks { get; set; } = 0;

        // Rank among all students in this exam
        public int? Rank { get; set; }

        // Percentage score
        [Range(0, 100)]
        public decimal Percentage { get; set; } = 0;

        // Status: Pending, Completed, Graded
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Graded

        // Track if result is published (visible to student)
        public bool IsPublished { get; set; } = false;

        [ForeignKey("EvaluatorUser")]
        public int? EvaluatedBy { get; set; }
        public User? EvaluatorUser { get; set; }

        public DateTime? EvaluatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Track when result was published
        public DateTime? PublishedAt { get; set; }
    }
}