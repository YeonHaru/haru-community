using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using haru_community.Data;
using haru_community.Models;
using haru_community.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace haru_community.Controllers;

[Authorize]
public class BoardPostsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BoardPostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? boardId)
    {
        if (boardId is null)
        {
            return RedirectToAction(nameof(BoardsController.Index), "Boards");
        }

        var board = await _context.Boards
            .Include(b => b.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board is null)
        {
            return NotFound();
        }

        var posts = await _context.BoardPosts
            .Where(p => p.BoardId == board.Id)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var viewModel = new BoardDetailsViewModel
        {
            Board = board,
            Posts = posts
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var boardPost = await _context.BoardPosts
            .Include(p => p.Board)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (boardPost is null)
        {
            return NotFound();
        }

        return View(boardPost);
    }

    public async Task<IActionResult> Create(int? boardId)
    {
        var boardOptions = await BuildBoardOptionsAsync(boardId);
        if (boardOptions.Count == 0)
        {
            TempData["BoardCreationNotice"] = "게시판을 먼저 만들어 주세요.";
            return RedirectToAction(nameof(BoardsController.Create), "Boards");
        }

        if (!boardOptions.Any(option => option.Selected))
        {
            boardOptions[0].Selected = true;
        }

        var selectedOption = boardOptions.First(option => option.Selected);
        var model = new BoardPost
        {
            BoardId = int.Parse(selectedOption.Value!, CultureInfo.InvariantCulture)
        };

        ViewBag.BoardOptions = boardOptions;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BoardPost boardPost)
    {
        ModelState.Remove(nameof(BoardPost.AuthorId));
        ModelState.Remove(nameof(BoardPost.AuthorName));

        var board = await _context.Boards.FindAsync(boardPost.BoardId);
        if (board is null)
        {
            ModelState.AddModelError(nameof(BoardPost.BoardId), "선택한 게시판을 찾을 수 없습니다.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BoardOptions = await BuildBoardOptionsAsync(boardPost.BoardId);
            return View(boardPost);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        boardPost.AuthorId = user.Id;
        boardPost.AuthorName = user.UserName ?? user.Email ?? "사용자";
        boardPost.CreatedAt = DateTime.UtcNow;

        _context.Add(boardPost);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { boardId = boardPost.BoardId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var boardPost = await _context.BoardPosts
            .Include(p => p.Board)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (boardPost is null)
        {
            return NotFound();
        }

        if (!CanManagePost(boardPost))
        {
            return Forbid();
        }

        ViewData["BoardName"] = boardPost.Board?.Name;
        return View(boardPost);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BoardPost boardPost)
    {
        if (id != boardPost.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewData["BoardName"] = (await _context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == boardPost.BoardId))?.Name;
            return View(boardPost);
        }

        var existingPost = await _context.BoardPosts
            .Include(p => p.Board)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (existingPost is null)
        {
            return NotFound();
        }

        if (!CanManagePost(existingPost))
        {
            return Forbid();
        }

        boardPost.BoardId = existingPost.BoardId;
        boardPost.AuthorId = existingPost.AuthorId;
        boardPost.AuthorName = existingPost.AuthorName;
        boardPost.CreatedAt = existingPost.CreatedAt;
        boardPost.UpdatedAt = DateTime.UtcNow;

        _context.Update(boardPost);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { boardId = boardPost.BoardId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var boardPost = await _context.BoardPosts
            .Include(p => p.Board)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (boardPost is null)
        {
            return NotFound();
        }

        if (!CanManagePost(boardPost))
        {
            return Forbid();
        }

        return View(boardPost);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var boardPost = await _context.BoardPosts
            .Include(p => p.Board)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (boardPost is null)
        {
            return RedirectToAction(nameof(BoardsController.Index), "Boards");
        }

        if (!CanManagePost(boardPost))
        {
            return Forbid();
        }

        _context.BoardPosts.Remove(boardPost);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { boardId = boardPost.BoardId });
    }

    private bool CanManagePost(BoardPost post)
    {
        if (User.IsInRole("Administrator"))
        {
            return true;
        }

        var currentUserId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return false;
        }

        if (string.Equals(post.AuthorId, currentUserId, StringComparison.Ordinal))
        {
            return true;
        }

        if (post.Board is not null && !string.IsNullOrEmpty(post.Board.OwnerId))
        {
            return string.Equals(post.Board.OwnerId, currentUserId, StringComparison.Ordinal);
        }

        return false;
    }

    private async Task<List<SelectListItem>> BuildBoardOptionsAsync(int? selectedBoardId = null)
    {
        var boards = await _context.Boards
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync();

        return boards
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(CultureInfo.InvariantCulture),
                Text = b.Name,
                Selected = selectedBoardId.HasValue && b.Id == selectedBoardId.Value
            })
            .ToList();
    }
}
