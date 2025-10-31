namespace QuizPortalAPI.DTOs.StudentResponse
{
    public class QuestionStatisticsDTO
    {
        public int QuestionID { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public int TotalResponses { get; set; }

        public int CorrectResponses { get; set; }

        public int IncorrectResponses { get; set; }

        public int UnevaluatedResponses { get; set; }

        public decimal CorrectPercentage { get; set; }

        public double AverageMarksObtained { get; set; }
    }
}