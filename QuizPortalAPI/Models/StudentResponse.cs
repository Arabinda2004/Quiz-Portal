using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuizPortalAPI.Models
{
    [Index(nameof(ExamID), nameof(QuestionID), nameof(StudentID), IsUnique = true, Name = "IX_StudentResponse_Unique")]
    public class StudentResponse
    {
        [Key]
        public int ResponseID { get; set; }

        [Required]
        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        [Required]
        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public Question? Question { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentID { get; set; }
        public User? Student { get; set; }

        [Column(TypeName = "text")]
        public string AnswerText { get; set; } = string.Empty;

        // For MCQ: True/False, For Subjective: Null until evaluated
        public bool? IsCorrect { get; set; }

        // Marks obtained for this response (0 for incorrect MCQ, marks for correct MCQ)
        [Range(0, 100)]
        public decimal MarksObtained { get; set; } = 0;

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}