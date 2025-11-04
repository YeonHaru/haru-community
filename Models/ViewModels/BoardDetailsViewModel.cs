using System.Collections.Generic;

namespace haru_community.Models.ViewModels;

public class BoardDetailsViewModel
{
    public Board Board { get; init; } = null!;

    public IReadOnlyCollection<BoardPost> Posts { get; init; } = System.Array.Empty<BoardPost>();
}
