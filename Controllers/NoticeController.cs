using System;
using System.Collections.Generic;
using System.Linq;
using haru_community.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace haru_community.Controllers;

[AllowAnonymous]
public class NoticeController : Controller
{
    private static readonly IReadOnlyList<NoticeViewModel> DefaultNotices = new[]
    {
        new NoticeViewModel
        {
            Id = 1,
            Title = "HARU Community 베타 오픈 안내",
            Summary = "커뮤니티 정식 오픈 이전 베타 기간이 시작됐습니다.",
            Content = "HARU Community 베타를 찾아주셔서 감사합니다. 베타 기간 동안 게시판과 회원 기능을 점검하고 있으니, 발견하신 문제는 마이페이지의 문의 기능을 통해 알려주세요.",
            PublishedAt = new DateTime(2025, 2, 1),
            IsPinned = true
        },
        new NoticeViewModel
        {
            Id = 2,
            Title = "이미지 업로드 정책 변경",
            Summary = "용량 제한이 5MB로 상향되고, 지원 포맷이 확대되었습니다.",
            Content = "보다 원활한 게시판 활동을 위해 이미지 업로드 용량 제한을 5MB로, 지원 포맷을 PNG/JPEG/GIF로 확장했습니다. 부적절한 이미지는 운영진이 사전 안내 후 조치할 수 있습니다.",
            PublishedAt = new DateTime(2025, 1, 25),
            IsPinned = false
        },
        new NoticeViewModel
        {
            Id = 3,
            Title = "정기 점검 예정",
            Summary = "2월 15일(토) 새벽 1시~3시 서비스 점검이 진행됩니다.",
            Content = "더 안정적인 서비스를 제공하기 위해 2월 15일 새벽 1시부터 3시까지 시스템 점검을 진행합니다. 점검 시간 동안 서비스 접속이 일시적으로 제한됩니다.",
            PublishedAt = new DateTime(2025, 1, 20),
            IsPinned = false
        }
    };

    public IActionResult Index()
    {
        var notices = DefaultNotices
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.PublishedAt)
            .ToList();

        ViewData["Title"] = "공지사항";

        return View(notices);
    }
}
