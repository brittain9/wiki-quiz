using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models // Or your specific namespace
{
    public class User
    {
        public int Id { get; set; } // Primary Key (or Guid, string)

        [MaxLength(200)]
        public string? GoogleId { get; set; } // Store Google's unique identifier

        [Required]
        [MaxLength(256)]
        public required string Email { get; set; } // User's email

        [MaxLength(200)]
        public string? DisplayName { get; set; } // User's display name

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Add other properties relevant to your application
        // public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}