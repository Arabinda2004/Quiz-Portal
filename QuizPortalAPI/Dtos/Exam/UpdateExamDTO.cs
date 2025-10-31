using System;
using System.ComponentModel.DataAnnotations;

namespace QuizPortalAPI.DTOs.Exam
{
    public class UpdateExamDTO
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Batch remark cannot exceed 100 characters")]
        public string? BatchRemark { get; set; }

        [Range(1, 480, ErrorMessage = "Duration must be between 1 and 480 minutes")]
        public int? DurationMinutes { get; set; }

        public DateTime? ScheduleStart { get; set; }

        public DateTime? ScheduleEnd { get; set; }

        [Range(0.01, 10000, ErrorMessage = "Total marks must be greater than 0")]
        public decimal? TotalMarks { get; set; }

        [Range(0, 10000, ErrorMessage = "Passing marks must be 0 or greater")]
        public decimal? PassingMarks { get; set; }

        public bool? HasNegativeMarking { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string? AccessPassword { get; set; }
    }
}