using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuizPortalAPI.Models
{
    public enum QuestionType
    {
        MCQ = 0,     
        SAQ = 1,   
        Subjective = 2   
    }

    [Index(nameof(ExamID))]
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }

        [Required]
        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Range(0.1, 100)]
        public decimal Marks { get; set; }

        [Range(0, 100)]
        public decimal NegativeMarks { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for options
        public ICollection<QuestionOption>? Options { get; set; }
    }
}