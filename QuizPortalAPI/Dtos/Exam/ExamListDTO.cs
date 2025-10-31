using System;

namespace QuizPortalAPI.DTOs.Exam
{
    public class ExamListDTO
    {
        public int ExamID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? BatchRemark { get; set; }

        public string CreatedByName { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public DateTime ScheduleStart { get; set; }

        public DateTime ScheduleEnd { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }  // True if current time is between ScheduleStart and ScheduleEnd

        public string Status { get; set; } = "Upcoming";  // Upcoming, Active, Completed

        public int? TotalQuestions { get; set; }  // Will be populated later when we add questions

        public decimal PassingPercentage { get; set; }

        public decimal TotalMarks { get; set; }  // Computed from questions

        public decimal PassingMarks { get; set; }  // Computed from TotalMarks * PassingPercentage
    }
}