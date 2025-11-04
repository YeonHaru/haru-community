using System;
using System.Linq;
using System.Threading.Tasks;
using haru_community.Data;
using haru_community.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace haru_community.Controllers;

[Authorize]
public class BoardsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BoardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var boards = await _context.Boards
            .Include(b => b.Owner)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(boards);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var boardExists = await _context.Boards.AsNoTracking().AnyAsync(b => b.Id == id);

        if (!boardExists)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(BoardPostsController.Index), "BoardPosts", new { boardId = id });
    }

    public IActionResult Create()
    {
        return View(new Board());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Board board)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        ModelState.Remove(nameof(Board.OwnerId));

        if (!ModelState.IsValid)
        {
            return View(board);
        }

        board.OwnerId = user.Id;
        board.CreatedAt = DateTime.UtcNow;

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = board.Id });
    }
}
