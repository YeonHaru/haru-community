using System;
using System.Linq;
using System.Threading.Tasks;
using haru_community.Data;
using haru_community.Models;
using haru_community.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace haru_community.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalBoards = await _context.Boards.CountAsync(),
            TotalPosts = await _context.BoardPosts.CountAsync()
        };

        ViewData["Title"] = "관리자 대시보드";

        return View(model);
    }

    public async Task<IActionResult> Users()
    {
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        var adminUserIds = adminRole is null
            ? Array.Empty<string>()
            : await _context.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToArrayAsync();

        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new AdminUserViewModel
            {
                Id = u.Id,
                UserName = u.UserName ?? "(이름 없음)",
                Email = u.Email ?? "(이메일 없음)",
                EmailConfirmed = u.EmailConfirmed,
                IsAdmin = adminUserIds.Contains(u.Id)
            })
            .ToListAsync();

        ViewData["Title"] = "사용자 관리";

        return View(users);
    }

    public async Task<IActionResult> Boards()
    {
        var boards = await _context.Boards
            .Include(b => b.Owner)
            .Select(b => new AdminBoardViewModel
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                OwnerName = b.Owner != null ? (b.Owner.UserName ?? b.Owner.Email ?? "알 수 없음") : "알 수 없음",
                CreatedAt = b.CreatedAt,
                PostCount = b.Posts.Count
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        ViewData["Title"] = "게시판 관리";

        return View(boards);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board is null)
        {
            TempData["AdminStatusMessage"] = "삭제할 게시판을 찾을 수 없습니다.";
            return RedirectToAction(nameof(Boards));
        }

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        TempData["AdminStatusMessage"] = $"게시판 '{board.Name}'을(를) 삭제했습니다.";

        return RedirectToAction(nameof(Boards));
    }
}
