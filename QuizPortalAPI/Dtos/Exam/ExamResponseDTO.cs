using System;

namespace QuizPortalAPI.DTOs.Exam
{
    public class ExamResponseDTO
    {
        public int ExamID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CreatedBy { get; set; }

        public string? CreatedByUserName { get; set; }

        public string? BatchRemark { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime ScheduleStart { get; set; }

        public DateTime ScheduleEnd { get; set; }

        public decimal PassingPercentage { get; set; }

        public decimal TotalMarks { get; set; }  // Computed from questions

        public decimal PassingMarks { get; set; }  // Computed from TotalMarks * PassingPercentage

        public bool HasNegativeMarking { get; set; }

        public string AccessCode { get; set; } = string.Empty;

        // ✅ Password should NOT be returned to frontend for security
        // public string AccessPassword { get; set; }  // REMOVED for security

        public DateTime CreatedAt { get; set; }

        // public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; }  // True if current time is between ScheduleStart and ScheduleEnd

        public TimeSpan? TimeUntilStart { get; set; }

        public TimeSpan? TimeRemaining { get; set; }
    }
}