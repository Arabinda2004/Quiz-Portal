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

        public decimal TotalMarks { get; set; }  

        public decimal PassingMarks { get; set; }  

        public string AccessCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } 

        public TimeSpan? TimeUntilStart { get; set; }

        public TimeSpan? TimeRemaining { get; set; }
    }
}