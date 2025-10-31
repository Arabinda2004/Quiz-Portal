namespace QuizPortalAPI.DTOs.StudentResponse
{
    public class StudentExamResponsesDTO
    {
        public int ExamID { get; set; }

        public string ExamName { get; set; } = string.Empty;

        public int TotalQuestions { get; set; }

        public int AnsweredQuestions { get; set; }

        public int UnansweredQuestions { get; set; }

        public List<StudentResponseDTO> Responses { get; set; } = new();
    }
}