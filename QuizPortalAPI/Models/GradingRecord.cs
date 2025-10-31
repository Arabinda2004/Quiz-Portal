using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    public class GradingRecord
    {
        [Key]
        public int GradingID { get; set; }

        [Required]
        [ForeignKey("StudentResponse")]
        public int ResponseID { get; set; }
        public StudentResponse? StudentResponse { get; set; }

        [Required]
        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public Question? Question { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentID { get; set; }
        public User? Student { get; set; }

        [Required]
        [ForeignKey("GradedBy")]
        public int GradedByTeacherID { get; set; }
        public User? GradedByTeacher { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal MarksObtained { get; set; }

        [Column(TypeName = "text")]
        public string? Feedback { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        // Whether this response received partial credit
        public bool IsPartialCredit { get; set; } = false;

        // Status: Graded, Pending, Regraded
        [Required]
        public string Status { get; set; } = "Graded";

        // Track if this grading was revised
        public int? RegradeFrom { get; set; }

        [Column(TypeName = "text")]
        public string? RegradeReason { get; set; }

        [Required]
        public DateTime GradedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RegradeAt { get; set; }
    }
}