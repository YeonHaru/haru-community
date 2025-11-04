using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace haru_community.Models;

public class Board
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BoardPost> Posts { get; set; } = new List<BoardPost>();
}
