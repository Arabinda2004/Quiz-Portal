// For students accessing exam 

using System;

namespace QuizPortalAPI.DTOs.Exam
{
    public class ExamAccessResponseDTO
    {
        public int ExamID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime ScheduleStart { get; set; }

        public DateTime ScheduleEnd { get; set; }

        public decimal PassingPercentage { get; set; }

        public decimal TotalMarks { get; set; }  // Computed from questions

        public decimal PassingMarks { get; set; }  // Computed from TotalMarks * PassingPercentage

        public string? BatchRemark { get; set; }

        public string CreatedByUserName { get; set; } = string.Empty;

        public bool CanAttempt { get; set; }  // True if current time is within schedule and not already attempted

        public string Message { get; set; } = string.Empty;  // Info message for student
    }
}