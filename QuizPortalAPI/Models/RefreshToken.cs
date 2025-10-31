using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPortalAPI.Models
{
    /// <summary>
    /// Model for storing refresh tokens in the database
    /// Used to invalidate tokens on logout or when user changes password
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public int RefreshTokenId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public bool IsRevoked => RevokedAt != null;
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsRevoked && !IsExpired;
    }
}
