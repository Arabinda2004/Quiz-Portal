namespace QuizPortalAPI.DTOs.StudentResponse
{
    /// <summary>
    /// DTO for paginated question responses
    /// </summary>
    public class QuestionResponsesPagedDTO
    {
        /// <summary>
        /// Total count of responses
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of responses
        /// </summary>
        public IEnumerable<QuestionResponseDTO> Responses { get; set; } = new List<QuestionResponseDTO>();
    }
}
