namespace QuizPortalAPI.DTOs.StudentResponse
{
    /// <summary>
    /// DTO for exam-wide statistics
    /// </summary>
    public class ExamStatisticsDTO
    {
        /// <summary>
        /// Exam ID
        /// </summary>
        public int ExamId { get; set; }

        /// <summary>
        /// Exam Name
        /// </summary>
        public string ExamName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of enrolled students
        /// </summary>
        public int TotalStudents { get; set; }

        /// <summary>
        /// Students who actually attempted the exam
        /// </summary>
        public int StudentsAttempted { get; set; }

        /// <summary>
        /// Students who did not attempt
        /// </summary>
        public int StudentsNotAttempted { get; set; }

        /// <summary>
        /// Average score across all students
        /// </summary>
        public decimal AverageScore { get; set; }

        /// <summary>
        /// Highest score obtained
        /// </summary>
        public int HighestScore { get; set; }

        /// <summary>
        /// Lowest score obtained
        /// </summary>
        public int LowestScore { get; set; }

        /// <summary>
        /// Percentage of students who passed
        /// </summary>
        public decimal PassPercentage { get; set; }

        /// <summary>
        /// Total marks for the exam
        /// </summary>
        public int TotalMarks { get; set; }

        /// <summary>
        /// Passing marks for the exam
        /// </summary>
        public int PassingMarks { get; set; }

        /// <summary>
        /// Score distribution (ranges and counts)
        /// </summary>
        public Dictionary<string, int> ScoreDistribution { get; set; } = new();

        /// <summary>
        /// Analysis of each question
        /// </summary>
        public IEnumerable<QuestionAnalysisDTO> QuestionAnalysis { get; set; } = new List<QuestionAnalysisDTO>();
    }

    /// <summary>
    /// Individual question analysis in exam statistics
    /// </summary>
    public class QuestionAnalysisDTO
    {
        /// <summary>
        /// Question ID
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Question text
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Percentage of students who answered correctly
        /// </summary>
        public decimal CorrectPercentage { get; set; }

        /// <summary>
        /// Difficulty level of question (Easy/Medium/Hard)
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// How many students answered this question
        /// </summary>
        public int AttemptedCount { get; set; }

        /// <summary>
        /// How many skipped this question
        /// </summary>
        public int SkippedCount { get; set; }
    }
}
