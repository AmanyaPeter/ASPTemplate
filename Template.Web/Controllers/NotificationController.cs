using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
}
