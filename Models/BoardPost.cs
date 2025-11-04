using System.ComponentModel.DataAnnotations;

namespace haru_community.Models
{
    public class BoardPost
    {
        public int Id { get; set; }

        [Required]
        public int BoardId { get; set; }

        public Board? Board { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        public ApplicationUser? Author { get; set; }

        [StringLength(100)]
        public string AuthorName { get; set; } = "Anonymous";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
