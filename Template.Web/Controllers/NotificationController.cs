using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Web.Models.Notification;

namespace Template.Web.Controllers;

[Authorize]
public class NotificationController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string filter = "all", int page = 1)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        const int pageSize = 10;
        var query = context.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId);

        query = filter switch
        {
            "read" => query.Where(notification => notification.IsRead),
            "unread" => query.Where(notification => !notification.IsRead),
            _ => query
        };

        var count = await query.CountAsync();
        var model = new NotificationsModel
        {
            Filter = filter,
            CurrentPage = Math.Max(page, 1),
            TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)),
            UnreadCount = await context.Notifications
                .AsNoTracking()
                .CountAsync(notification =>
                    notification.UserId == userId && !notification.IsRead),
            Notifications = await query
                .OrderByDescending(notification => notification.CreatedDate)
                .Skip((Math.Max(page, 1) - 1) * pageSize)
                .Take(pageSize)
                .Select(notification => new NotificationItemViewModel
                {
                    Id = notification.Id,
                    Subject = notification.Subject,
                    Message = notification.Message,
                    Icon = notification.Type.ToString(),
                    TimeAgo = notification.CreatedDate.ToString("dd MMM yyyy HH:mm"),
                    IsRead = notification.IsRead
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
    public async Task<IActionResult> MarkRead(Guid id, string filter = "all")
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(n => n.Id == id && n.UserId == UserId());
        if (notification == null) return NotFound();
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { filter });
=======
    public async Task<IActionResult> MarkRead(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var notification = await context.Notifications.FirstOrDefaultAsync(item =>
            item.Id == id && item.UserId == userId);
        if (notification == null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
>>>>>>> dev
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
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
=======
    public async Task<IActionResult> MarkAllRead()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var notifications = await context.Notifications
            .Where(item => item.UserId == userId && !item.IsRead)
            .ToListAsync();
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
>>>>>>> dev
}
