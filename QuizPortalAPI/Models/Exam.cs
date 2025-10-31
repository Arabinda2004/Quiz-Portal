using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuizPortalAPI.Models
{
    [Index(nameof(AccessCode), IsUnique = true)]
    public class Exam
    {
        [Key]
        public int ExamID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }

        [StringLength(100)]
        public string? BatchRemark { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public DateTime ScheduleStart { get; set; }

        [Required]
        public DateTime ScheduleEnd { get; set; }

        public bool HasNegativeMarking { get; set; } = false;

        [Required]
        public decimal TotalMarks { get; set; } = 0;

        [Required]
        public decimal PassingMarks { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string AccessCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccessPassword { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for questions
        public ICollection<Question>? Questions { get; set; }
    }
}