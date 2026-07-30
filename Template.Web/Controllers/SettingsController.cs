using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartBreadcrumbs.Attributes;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Hubs;
using Template.Web.Models.Settings;

namespace Template.Web.Controllers;

[Authorize]
public class SettingsController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IHubContext<ImtsHub> hub) : Controller
{
    [Breadcrumb("System Settings", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Index() => User.IsInRole(RoleConstants.ItAdmin) ? View() : Forbid();

    [HttpGet]
    [Breadcrumb("Help & Support", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Support() =>
        User.IsInRole(RoleConstants.ItAdmin)
            ? Forbid()
            : View(new SupportRequestViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSupportRequest(
        SupportRequestViewModel model,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleConstants.ItAdmin))
        {
            return Forbid();
        }

        if (!new[] { "Low", "Normal", "High", "Urgent" }.Contains(model.Priority))
        {
            ModelState.AddModelError(nameof(model.Priority), "Select a valid priority.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Help & Support";
            return View(nameof(Support), model);
        }

        var sender = await userManager.GetUserAsync(User);
        if (sender == null)
        {
            return Forbid();
        }

        var administrators = await userManager.GetUsersInRoleAsync(RoleConstants.ItAdmin);
        var activeAdministrators = administrators.Where(admin => admin.IsActive).ToList();
        if (activeAdministrators.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No active administrator is available. Please contact IT support directly.");
            return View(nameof(Support), model);
        }

        var requestId = Guid.NewGuid();
        var subject = model.Subject.Trim();
        var message = model.Message.Trim();
        var senderName = string.IsNullOrWhiteSpace(sender.FullName)
            ? sender.UserName ?? "A user"
            : sender.FullName;

        foreach (var administrator in activeAdministrators)
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = administrator.Id,
                Type = NotificationType.SupportRequest,
                Subject = $"Support request: {subject}",
                Message = $"{senderName} submitted a {model.Priority.ToLowerInvariant()} priority request: {message}",
                LinkUrl = "/Notification",
                CreatedDate = DateTime.UtcNow
            });

            if (!string.IsNullOrWhiteSpace(administrator.Email))
            {
                db.EmailOutbox.Add(new EmailOutbox
                {
                    Id = Guid.NewGuid(),
                    IdempotencyKey = $"support:{requestId}:{administrator.Id}",
                    Recipient = administrator.Email,
                    Subject = $"[{model.Priority}] IMTS support request: {subject}",
                    Body = $"From: {senderName} ({sender.UserName}, {sender.Email})\nPriority: {model.Priority}\n\n{message}",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        foreach (var administrator in activeAdministrators)
        {
            await hub.Clients.Group($"user:{administrator.Id}").SendAsync(
                "notificationChanged",
                new
                {
                    type = NotificationType.SupportRequest.ToString(),
                    subject = $"Support request: {subject}"
                },
                cancellationToken);
        }

        TempData["SuccessMessage"] = "Your message was sent to the system administrators.";
        return RedirectToAction(nameof(Support));
    }
}
