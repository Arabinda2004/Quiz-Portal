using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    public class QuestionOption
    {
        [Key]
        public int OptionID { get; set; }

        [Required]
        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public Question? Question { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string OptionText { get; set; } = string.Empty;

        [Required]
        public bool IsCorrect { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}