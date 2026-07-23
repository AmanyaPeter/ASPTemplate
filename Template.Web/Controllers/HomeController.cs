using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using Template.Data.Configurations;
using Template.Web.Models;
using Template.Web.Models.Notification;
using Template.Web.Models.Shared;

namespace Template.Web.Controllers;

[Authorize]
[DefaultBreadcrumb]
public class HomeController(ApplicationDbContext db, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            var users = await db.ApplicationUsers.AsNoTracking().ToListAsync();
            return View((object)new AdminDashboardModel
            {
                Stats = new AdminStatsViewModel
                {
                    TotalUsers = users.Count,
                    ActiveUsers = users.Count(u => u.IsActive),
                    LockedAccounts = users.Count(u => u.LockoutEnd > DateTimeOffset.UtcNow || !u.IsActive),
                    RecentActivities = await db.AuditLogs.CountAsync(a => a.CreatedDate >= DateTime.UtcNow.AddDays(-7))
                },
                RecentUsers = users.OrderByDescending(u => u.CreatedDate).Take(8).Select(u => new RecentUserViewModel
                {
                    Name = u.FullName, Email = u.Email, LastLogin = u.LastLoginDate ?? u.CreatedDate,
                    IsOnline = u.IsLoggedIn
                }).ToList()
            });
        }

        if (User.IsInRole("InnovationTeam"))
        {
            var ideas = await db.InnovationIdeas.AsNoTracking().Include(i => i.Submitter)
                .Where(i => !i.IsDeleted && !i.IsRetracted).OrderByDescending(i => i.SubmissionDate).ToListAsync();
            var deadlines = await db.IdeaTimelines.AsNoTracking().Include(t => t.Idea).Include(t => t.ApprovedBy)
                .Where(t => t.ActualCompletionDate == null).OrderBy(t => t.DeadlineDate).Take(8).ToListAsync();
            return View((object)new InnovationDashboardModel
            {
                Stats = new InnovationStatsViewModel
                {
                    TotalIdeas = ideas.Count,
                    PendingReviews = ideas.Count(i => i.CurrentStatus == "Under Review"),
                    ConceptDevelopment = ideas.Count(i => i.CurrentStage == "Concept Development"),
                    Experimentation = ideas.Count(i => i.CurrentStage == "Experimentation"),
                    Deployment = ideas.Count(i => i.CurrentStage == "Deployment")
                },
                RecentIdeas = ideas.Take(8).Select(i => new RecentIdeaViewModel
                {
                    Title = i.Title, Submitter = i.Submitter.FullName,
                    Status = i.CurrentStatus, Date = i.SubmissionDate
                }).ToList(),
                ReviewQueue = ideas.Where(i => i.CurrentStatus == "Under Review").Take(8)
                    .Select(i => new ReviewQueueItemViewModel
                    { Title = i.Title, StatusText = i.CurrentStatus, StatusClass = "warning" }).ToList(),
                SLADeadlines = deadlines.Select(t => new SLADeadlineViewModel
                {
                    Idea = t.Idea.Title, Reviewer = t.ApprovedBy?.FullName,
                    Deadline = t.DeadlineDate, Status = t.DeadlineDate < DateTime.UtcNow ? "Overdue" : "Open",
                    StatusClass = t.DeadlineDate < DateTime.UtcNow ? "danger" : "warning"
                }).ToList()
            });
        }

        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : Guid.Empty;
        var ownIdeas = await db.InnovationIdeas.AsNoTracking().Include(i => i.Category)
            .Where(i => i.SubmitterId == userId && !i.IsDeleted).OrderByDescending(i => i.SubmissionDate).ToListAsync();
        var notifications = await db.Notifications.AsNoTracking().Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate).Take(5).ToListAsync();
        return View((object)new StaffDashboardModel
        {
            TotalIdeas = ownIdeas.Count,
            UnderReview = ownIdeas.Count(i => i.CurrentStatus == "Under Review"),
            Approved = ownIdeas.Count(i => i.CurrentStatus == "Approved"),
            Completed = ownIdeas.Count(i => i.CurrentStage == "Closed"),
            RecentSubmissions = ownIdeas.Take(5).Select(i => new RecentSubmissionViewModel
            {
                Title = i.Title, Category = i.Category?.Name, Status = i.CurrentStatus, Date = i.SubmissionDate
            }).ToList(),
            RecentNotifications = notifications.Select(n => new NotificationItemViewModel
            {
                Id = n.Id, Subject = n.Subject, Message = n.Message, IsRead = n.IsRead,
                Icon = "bell", TimeAgo = n.CreatedDate.ToString("dd MMM yyyy")
            }).ToList()
        });
    }

    public IActionResult TestPage() => View();

    [Breadcrumb("UI Kit", FromAction = nameof(Index))]
    public IActionResult UiKit() => View();

    [Breadcrumb("Privacy", FromAction = nameof(Index))]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("Unhandled request error. Trace identifier {TraceId}", HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
