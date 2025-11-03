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
        [Range(0, 100)]
        public decimal PassingPercentage { get; set; } = 40;

        // Computed property: TotalMarks calculated from sum of all question marks
        [NotMapped]
        public decimal TotalMarks
        {
            get
            {
                return Questions?.Sum(q => q.Marks) ?? 0;
            }
        }

        // Computed property: PassingMarks calculated from TotalMarks and PassingPercentage
        [NotMapped]
        public decimal PassingMarks
        {
            get
            {
                return (TotalMarks * PassingPercentage) / 100;
            }
        }

        [Required]
        [StringLength(50)]
        public string AccessCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccessPassword { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // public DateTime? UpdatedAt { get; set; }

        // Navigation property for questions
        public ICollection<Question>? Questions { get; set; }
    }
}