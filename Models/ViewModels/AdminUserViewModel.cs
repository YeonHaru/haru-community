using System;

namespace haru_community.Models.ViewModels;

public class AdminUserViewModel
{
    public string Id { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool EmailConfirmed { get; init; }

    public bool IsAdmin { get; init; }
}
