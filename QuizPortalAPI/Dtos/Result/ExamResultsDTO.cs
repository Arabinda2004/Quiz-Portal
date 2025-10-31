namespace QuizPortalAPI.DTOs.Result
{
    public class ExamResultsDTO
    {
        public int ExamID { get; set; }

        public string ExamName { get; set; } = string.Empty;

        public int TotalSubmissions { get; set; }

        public int GradedSubmissions { get; set; }

        public decimal AverageMarks { get; set; }

        public decimal HighestMarks { get; set; }

        public decimal LowestMarks { get; set; }

        public List<ResultDTO> Results { get; set; } = new();
    }
}