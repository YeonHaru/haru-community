using System;

namespace haru_community.Models.ViewModels;

public class NoticeViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;

    public bool IsPinned { get; init; }
}
