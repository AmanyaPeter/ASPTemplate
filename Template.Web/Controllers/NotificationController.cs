using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Web.Models.Notification;

namespace Template.Web.Controllers;

[Authorize]
public class NotificationController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 15;

    [HttpGet]
    public async Task<IActionResult> Index(string filter = "all", int page = 1)
    {
        var userId = UserId();
        if (userId == null) return Challenge();
        page = Math.Max(page, 1);
        var query = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (filter == "unread") query = query.Where(n => !n.IsRead);
        if (filter == "read") query = query.Where(n => n.IsRead);
        var count = await query.CountAsync();
        var items = await query.OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(n => new NotificationItemViewModel
            {
                Id = n.Id, Subject = n.Subject, Message = n.Message,
                IsRead = n.IsRead, Icon = IconFor(n.Type), TimeAgo = RelativeTime(n.CreatedDate)
            }).ToListAsync();
        return View(new NotificationsModel
        {
            Filter = filter, Notifications = items,
            UnreadCount = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id, string filter = "all")
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(n => n.Id == id && n.UserId == UserId());
        if (notification == null) return NotFound();
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(string filter = "all")
    {
        var notifications = await db.Notifications.Where(n => n.UserId == UserId() && !n.IsRead).ToListAsync();
        foreach (var item in notifications) { item.IsRead = true; item.ReadAt = DateTime.UtcNow; }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { filter });
    }

    private Guid? UserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static string IconFor(NotificationType type) => type switch
    {
        NotificationType.CommentAdded => "message-circle",
        NotificationType.ApprovalDecision => "check-circle",
        NotificationType.DeadlineReminder => "clock",
        NotificationType.SystemAnnouncement => "megaphone",
        _ => "bell"
    };

    private static string RelativeTime(DateTime value)
    {
        var span = DateTime.UtcNow - value.ToUniversalTime();
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalHours < 1) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalDays < 1) return $"{(int)span.TotalHours}h ago";
        return $"{(int)span.TotalDays}d ago";
    }
}
