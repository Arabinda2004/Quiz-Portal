using System;
using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Exam
{
    public class CreateExamDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Batch remark cannot exceed 100 characters")]
        public string? BatchRemark { get; set; }

        [Required(ErrorMessage = "Duration in minutes is required")]
        [Range(1, 480, ErrorMessage = "Duration must be between 1 and 480 minutes")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Schedule start time is required")]
        public DateTime ScheduleStart { get; set; }

        [Required(ErrorMessage = "Schedule end time is required")]
        public DateTime ScheduleEnd { get; set; }

        [Required(ErrorMessage = "Total marks is required")]
        [Range(0.01, 10000, ErrorMessage = "Total marks must be greater than 0")]
        public decimal TotalMarks { get; set; }

        [Required(ErrorMessage = "Passing marks is required")]
        [Range(0, 10000, ErrorMessage = "Passing marks must be 0 or greater")]
        public decimal PassingMarks { get; set; }

        public bool HasNegativeMarking { get; set; } = false;

        [Required(ErrorMessage = "Access password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string AccessPassword { get; set; } = string.Empty;
    }
}