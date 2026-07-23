using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Core.Models.Dashboard;
using Template.Data.Configurations;

namespace Template.Core.Repository.Dashboard;

public class DashboardRepository(ApplicationDbContext context) : IDashboardRepository
{
    public async Task<StaffDashboardViewModel> GetStaffDashboardAsync(Guid userId)
    {
        var ideas = context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => idea.SubmitterId == userId && !idea.IsDeleted);

        return new StaffDashboardViewModel
        {
            TotalIdeas = await ideas.CountAsync(),
            IdeasUnderReview = await ideas.CountAsync(idea =>
                !idea.IsRetracted && idea.CurrentStatus == nameof(IdeaStatus.UnderReview)),
            ApprovedIdeas = await ideas.CountAsync(idea =>
                !idea.IsRetracted && idea.CurrentStatus == nameof(IdeaStatus.Approved)),
            CompletedIdeas = await ideas.CountAsync(idea =>
                !idea.IsRetracted && idea.CurrentStage == nameof(IdeaStage.Closed)),
            RecentIdeas = await ideas
                .OrderByDescending(idea => idea.SubmissionDate)
                .Take(5)
                .Select(idea => new DashboardIdeaViewModel
                {
                    Id = idea.Id,
                    ReferenceNumber = idea.ReferenceNumber ?? string.Empty,
                    Title = idea.Title ?? string.Empty,
                    Category = idea.Category != null ? idea.Category.Name : "Uncategorized",
                    Stage = idea.CurrentStage ?? string.Empty,
                    Status = idea.CurrentStatus ?? string.Empty,
                    SubmissionDate = idea.SubmissionDate,
                    IsRetracted = idea.IsRetracted
                })
                .ToListAsync(),
            RecentNotifications = await context.Notifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId)
                .OrderByDescending(notification => notification.CreatedDate)
                .Take(5)
                .Select(notification => new DashboardNotificationViewModel
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Subject = notification.Subject,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedDate = notification.CreatedDate
                })
                .ToListAsync()
        };
    }

    public async Task<InnovationTeamDashboardViewModel> GetInnovationTeamDashboardAsync()
    {
        var activeIdeas = context.InnovationIdeas
            .AsNoTracking()
            .Where(idea =>
                !idea.IsDeleted &&
                !idea.IsRetracted &&
                idea.CurrentStage != nameof(IdeaStage.Closed) &&
                idea.CurrentStatus != nameof(IdeaStatus.Declined));

        return new InnovationTeamDashboardViewModel
        {
            TotalActiveIdeas = await activeIdeas.CountAsync(),
            PendingReviewIdeas = await activeIdeas.CountAsync(idea =>
                idea.CurrentStatus == nameof(IdeaStatus.UnderReview)),
            ConceptDevelopmentIdeas = await activeIdeas.CountAsync(idea =>
                idea.CurrentStage == nameof(IdeaStage.ConceptDevelopment)),
            ExperimentationIdeas = await activeIdeas.CountAsync(idea =>
                idea.CurrentStage == nameof(IdeaStage.Experimentation)),
            DeploymentIdeas = await activeIdeas.CountAsync(idea =>
                idea.CurrentStage == nameof(IdeaStage.Deployment)),
            ReviewQueue = await activeIdeas
                .Where(idea =>
                    idea.CurrentStatus == nameof(IdeaStatus.UnderReview) ||
                    idea.CurrentStage == nameof(IdeaStage.Submitted))
                .OrderBy(idea => idea.SubmissionDate)
                .Take(10)
                .Select(idea => new DashboardIdeaViewModel
                {
                    Id = idea.Id,
                    ReferenceNumber = idea.ReferenceNumber ?? string.Empty,
                    Title = idea.Title ?? string.Empty,
                    Category = idea.Category != null ? idea.Category.Name : "Uncategorized",
                    Stage = idea.CurrentStage ?? string.Empty,
                    Status = idea.CurrentStatus ?? string.Empty,
                    SubmissionDate = idea.SubmissionDate,
                    SubmitterName = idea.Submitter != null ? idea.Submitter.FullName : "Unknown",
                    SubmitterDepartment = idea.Submitter != null
                        ? idea.Submitter.BusinessUnit
                        : "Not provided"
                })
                .ToListAsync(),
            StageDeadlines = await context.IdeaTimelines
                .AsNoTracking()
                .Where(timeline =>
                    timeline.ActualCompletionDate == null &&
                    !timeline.Idea.IsDeleted &&
                    !timeline.Idea.IsRetracted)
                .OrderBy(timeline => timeline.DeadlineDate)
                .Take(10)
                .Select(timeline => new StageDeadlineViewModel
                {
                    IdeaId = timeline.IdeaId,
                    ReferenceNumber = timeline.Idea.ReferenceNumber ?? string.Empty,
                    IdeaTitle = timeline.Idea.Title ?? string.Empty,
                    Stage = timeline.Stage,
                    DeadlineDate = timeline.DeadlineDate,
                    IsOverdue = timeline.DeadlineDate < DateTime.UtcNow
                })
                .ToListAsync()
        };
    }

    public async Task<ItAdminDashboardViewModel> GetItAdminDashboardAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var users = context.Users.AsNoTracking();

        return new ItAdminDashboardViewModel
        {
            TotalUsers = await users.CountAsync(),
            ActiveUsers = await users.CountAsync(user =>
                user.IsActive &&
                (!user.DisableDate.HasValue || user.DisableDate > DateTime.UtcNow) &&
                (!user.EndDate.HasValue || user.EndDate > DateTime.UtcNow) &&
                (!user.LockoutEnd.HasValue || user.LockoutEnd <= now)),
            LockedAccounts = await users.CountAsync(user =>
                user.LockoutEnd.HasValue && user.LockoutEnd > now),
            DisabledAccounts = await users.CountAsync(user =>
                !user.IsActive ||
                (user.DisableDate.HasValue && user.DisableDate <= DateTime.UtcNow) ||
                (user.EndDate.HasValue && user.EndDate <= DateTime.UtcNow)),
            RecentUsers = await users
                .OrderByDescending(user => user.CreatedDate)
                .Take(5)
                .Select(user => new RecentUserViewModel
                {
                    Id = user.Id,
                    Name = user.FullName ?? user.UserName ?? "Unknown",
                    Email = user.Email ?? "Not provided",
                    IsActive = user.IsActive &&
                        (!user.DisableDate.HasValue || user.DisableDate > DateTime.UtcNow) &&
                        (!user.EndDate.HasValue || user.EndDate > DateTime.UtcNow),
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > now,
                    LastAccessTime = user.LastLoginDate ?? user.LastActivity,
                    CreatedDate = user.CreatedDate
                })
                .ToListAsync(),
            RecentAuditActivity = await context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(log => log.CreatedDate)
                .Take(10)
                .Select(log => new RecentAuditActivityViewModel
                {
                    Id = log.Id,
                    Username = log.Username ?? "Unknown",
                    EventType = log.EventType,
                    OperationPerformed = log.OperationPerformed ?? string.Empty,
                    Status = log.Status,
                    CreatedDate = log.CreatedDate
                })
                .ToListAsync()
        };
    }
}
