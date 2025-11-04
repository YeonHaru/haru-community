using System;

namespace haru_community.Models.ViewModels;

public class AdminBoardViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string OwnerName { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public int PostCount { get; init; }
}
